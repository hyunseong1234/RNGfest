using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabDataManager : MonoBehaviour
{



    public static PlayFabDataManager Instance;
    public string titleId = "18F60C";
    public string myPlayFabID;
    public UserGameData userData;

    public Action OnLoginSuccessEvent;  // 로그인 최종 완료 시 (UI 끄기용)
    public Action OnNeedSignUpEvent;    // 계정 없을 때 (가입 패널 띄우기용)

    private string _currentCustomId;
    private string _sessionKey;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        PlayFabSettings.staticSettings.TitleId = titleId;
        _currentCustomId = PlayerPrefs.GetString("CurrentCustomID", SystemInfo.deviceUniqueIdentifier);
        _sessionKey = SystemInfo.deviceUniqueIdentifier + "_" + DateTime.Now.Ticks;
    }

    private void OnApplicationQuit()
    {
        if (PlayFabClientAPI.IsClientLoggedIn() && userData != null && !userData._isDeleted)
        {
            userData._lastLoginId = "";
            SaveDataImmediate();
        }
    }

    // [자동 로그인 시도]
    public void CheckExistingAccount()
    {
        Debug.Log($"[로그인 확인] ID: {_currentCustomId}");
        var request = new LoginWithCustomIDRequest
        {
            CustomId = _currentCustomId,
            CreateAccount = false // 중요: 계정 없으면 에러를 뱉게 함
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, error =>
        {
            if (error.Error == PlayFabErrorCode.AccountNotFound)
            {
                Debug.LogWarning("계정 없음: 가입 패널을 요청합니다.");
                OnNeedSignUpEvent?.Invoke(); // UI 매니저에게 패널 띄우라고 신호
            }
            else { Debug.LogError(error.GenerateErrorReport()); }
        });
    }

    // [신규 가입 - 게스트]
    public void SignUpNewAccount()
    {
        // 신규 가입 시에는 ID를 새로 생성해서 충돌 방지
        string nextId = SystemInfo.deviceUniqueIdentifier + "_" + DateTime.Now.Ticks;
        PlayerPrefs.SetString("CurrentCustomID", nextId);
        PlayerPrefs.Save();
        _currentCustomId = nextId;

        var request = new LoginWithCustomIDRequest
        {
            CustomId = _currentCustomId,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        myPlayFabID = result.PlayFabId;
        LoadData();
    }
    /// <summary>
    /// 데이터 로드 함수
    /// </summary>
    public void LoadData()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, result =>
        {
            // 1. 서버에 데이터가 있는지 확인
            if (result.Data != null && result.Data.ContainsKey("PlayerStats"))
            {
                // 데이터가 있으면 불러오기만 함 (추가 작업 X)
                userData = JsonConvert.DeserializeObject<UserGameData>(result.Data["PlayerStats"].Value);

                if (userData._isDeleted) { SignUpNewAccount(); return; }

                // 세션 키만 업데이트 후 저장 (중복 로그인 체크용)
                userData._lastLoginId = _sessionKey;
                SaveData();
            }
            else
            {
                // 2. 데이터가 없으면 신규 유저로 간주하고 초기 설정 부여
                InitializeNewUser();
            }

            OnLoginSuccessEvent?.Invoke();
        }, OnLoginFailure);
    }

    /// <summary>
    /// 신규 가입유저의 초기 데이터 세팅 값
    /// </summary>
    private void InitializeNewUser()
    {
        // 처음 가입할 때만 딱 한 번 실행됨
        userData = new UserGameData();
        userData.SetDefaultValues(_sessionKey);

        SaveData(); // 서버에 초기 데이터 저장
    }

    public void RequestDeleteAccount()
    {
        userData._isDeleted = true;
        userData._lastLoginId = "";
        SaveDataImmediate();

        // ID 미리 갱신해서 다음 접속 시 신규 유저 취급받게 함
        PlayerPrefs.SetString("CurrentCustomID", SystemInfo.deviceUniqueIdentifier + "_" + DateTime.Now.Ticks);
        PlayerPrefs.Save();

        var scriptReq = new ExecuteCloudScriptRequest { FunctionName = "DeletePlayerAccount" };
        PlayFabClientAPI.ExecuteCloudScript(scriptReq, r => StartCoroutine(QuitApplicationAfterDelay(1.5f)), null);
    }

    public void SaveData()
    {
        if (userData == null) return;
        string json = JsonConvert.SerializeObject(userData);
        var request = new UpdateUserDataRequest { Data = new Dictionary<string, string> { { "PlayerStats", json } } };
        PlayFabClientAPI.UpdateUserData(request, null, null);
    }


    public void OnGachaResult(TowerType rewardType)
    {
        // 1. 이미 가지고 있는 타워인가?
        if (HasTower(rewardType))
        {
            // [중복 처리] 경험치를 주거나, 강화석으로 변환
            Debug.Log($"{rewardType} 중복 당첨! 조각이나 경험치로 변환합니다.");
            AddTowerExp(rewardType, 100);
        }
        else
        {
            // [신규 획득] 리스트에 새로 추가
            Debug.Log($"축하합니다! 신규 타워 {rewardType} 획득!");
            userData._towers.Add(new TowerGameData(rewardType, 1, 0));
        }

        // 2. 서버에 저장 (중요!)
        SaveData();
    }
    private bool HasTower(TowerType type)
    {
        // 리스트에서 해당 타입을 가진 타워가 하나라도 있는지 확인
        return userData._towers.Exists(t => t._id == type);
    }

    /// <summary>
    /// 뽑기했을때 중복 타워 경험치 주는 함수
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    public void AddTowerExp(TowerType type, int amount)
    {
        if (userData == null || userData._towers == null) return;

        // 리스트에서 해당 타입의 타워를 찾기
        TowerGameData target = userData._towers.Find(t => t._id == type);

        if (target != null)
        {
            target._currentExp += amount;

            SaveData();
        }
        else
        {
            Debug.LogWarning($"{type} 타워가 리스트에 없어 경험치를 추가할 수 없습니다.");
        }
    }

    private void SaveDataImmediate()
    {
        string json = JsonConvert.SerializeObject(userData);
        var request = new UpdateUserDataRequest { Data = new Dictionary<string, string> { { "PlayerStats", json } } };
        PlayFabClientAPI.UpdateUserData(request, null, null);
    }


    private void OnLoginFailure(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());
    private IEnumerator QuitApplicationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit(); 
#endif
    }
}



[Serializable]
public class UserGameData
{
    public int _gold = 0;
    public int _jewel = 0;
    public List<TowerGameData> _towers;
    public bool _isDeleted = false;
    public string _lastLoginId = "";

    public void SetDefaultValues(string sessionKey)
    {
        _gold = 0;
        _jewel = 0;

        _towers = new List<TowerGameData>
        {
            new TowerGameData(TowerType.Fire,1,0),
            new TowerGameData(TowerType.Archer,1,0),
            new TowerGameData(TowerType.Speed,1,0),
            new TowerGameData(TowerType.Electric,1,0),
            new TowerGameData(TowerType.Posion,1,0),

        };

        _isDeleted = false;

        _lastLoginId = sessionKey;
        Debug.Log("신규 유저 데이터 생성 완료");
    }


}

/// <summary>
/// 타워 종류 추가될때마다 여기다 이넘 추가해야됩니다.
/// </summary>
public enum TowerType
{
    None = 0,
    Fire = 1,
    Slow = 2,
    Archer = 3,
    Speed = 4,
    Electric = 5,
    Posion = 6,
    Stationary = 7,
    Marking = 8,
    Melee = 9,
    Buff = 10,
    Growth = 11,
    Adel = 12,


    Max = 9999
}

[Serializable]
public class TowerGameData
{
    public TowerType _id;
    public int _lv;
    public int _currentExp;
    public TowerGameData(TowerType id, int lv, int currentExp)
    {
        _id = id;
        _lv = lv;
        _currentExp = currentExp;
    }


}


