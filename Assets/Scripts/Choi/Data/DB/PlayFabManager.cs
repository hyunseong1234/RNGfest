using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabDataManager : MonoBehaviour
{
    [System.Serializable]
    public class UserGameData
    {
        public int gold = 500;
        public int high_score = 0;
        public List<int> towerLevels = new List<int> { 1, 1, 1, 1, 1 };
        public bool isDeleted = false;
        public string lastLoginId = "";
    }

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
        if (PlayFabClientAPI.IsClientLoggedIn() && userData != null && !userData.isDeleted)
        {
            userData.lastLoginId = "";
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

    public void LoadData()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerStats"))
            {
                userData = JsonConvert.DeserializeObject<UserGameData>(result.Data["PlayerStats"].Value);
                if (userData.isDeleted) { SignUpNewAccount(); return; }

                // 중복 로그인 체크
                if (!string.IsNullOrEmpty(userData.lastLoginId) && userData.lastLoginId != _sessionKey)
                {
                    Debug.LogWarning("중복 로그인 감지!");
                }
                userData.lastLoginId = _sessionKey;
                SaveData();
            }
            else { InitializeNewUser(); }

            OnLoginSuccessEvent?.Invoke();
        }, OnLoginFailure);
    }

    public void RequestDeleteAccount()
    {
        userData.isDeleted = true;
        userData.lastLoginId = "";
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

    private void SaveDataImmediate()
    {
        string json = JsonConvert.SerializeObject(userData);
        var request = new UpdateUserDataRequest { Data = new Dictionary<string, string> { { "PlayerStats", json } } };
        PlayFabClientAPI.UpdateUserData(request, null, null);
    }

    private void InitializeNewUser() { userData = new UserGameData(); userData.lastLoginId = _sessionKey; SaveData(); }
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