using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region PlayFabDataManager 
public class PlayFabDataManager : MonoBehaviour
{
    public static PlayFabDataManager Instance;
    public string titleId = "18F60C";
    public string myPlayFabID;
    public UserGameData userData;

    [Header("UI Reference")]
    [SerializeField] private LoadingImage _loadingPanel;

    private bool _isSaving = false;
    private int _retryCount = 0;
    private const int MAX_RETRY = 3;

    public Action OnLoginSuccessEvent;
    public Action OnNeedSignUpEvent;

    private string _currentCustomId;
    private string _sessionKey;
    private Coroutine _saveCoroutine; // 지연 저장용 코루틴 추적

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

    #region Login & Auth
    public void CheckExistingAccount()
    {
        var request = new LoginWithCustomIDRequest { CustomId = _currentCustomId, CreateAccount = false };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, error =>
        {
            if (error.Error == PlayFabErrorCode.AccountNotFound) OnNeedSignUpEvent?.Invoke();
            else Debug.LogError(error.GenerateErrorReport());
        });
    }

    public void SignUpNewAccount()
    {
        string nextId = SystemInfo.deviceUniqueIdentifier + "_" + DateTime.Now.Ticks;
        PlayerPrefs.SetString("CurrentCustomID", nextId);
        PlayerPrefs.Save();
        _currentCustomId = nextId;

        var request = new LoginWithCustomIDRequest { CustomId = _currentCustomId, CreateAccount = true };
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
                if (userData._isDeleted) { SignUpNewAccount(); return; }
                userData._lastLoginId = _sessionKey;
                SaveData(); // 로그인 시에는 즉시 저장
            }
            else InitializeNewUser();

            OnLoginSuccessEvent?.Invoke();
        }, OnLoginFailure);
    }

    private void InitializeNewUser()
    {
        userData = new UserGameData();
        userData.SetDefaultValues(_sessionKey);
        userData._userNickName = NameGenerator.Generate();
        SaveData();
    }
    #endregion

    #region Save Logic (Core)

    /// <summary>
    /// [신규] 유저 조작이 잦은 곳(덱 편집 등)에서 호출. 
    /// 2초간 추가 조작이 없으면 로딩바 없이 조용히 저장합니다.
    /// </summary>
    public void SaveDataWithDelay()
    {
        if (_saveCoroutine != null) StopCoroutine(_saveCoroutine);
        _saveCoroutine = StartCoroutine(Co_ReservedSave());
    }

    private IEnumerator Co_ReservedSave()
    {
        yield return new WaitForSeconds(2.0f);

        // 지연 저장 시에는 로딩바를 띄우지 않고 데이터만 전송
        ExecuteSave(showLoading: false);
        _saveCoroutine = null;
    }

    /// <summary>
    /// 기존 즉시 저장 함수. 가챠 등 중요 시점에서 호출하며 로딩바를 띄웁니다.
    /// </summary>
    public void SaveData()
    {
        if (userData == null || _isSaving) return;

        // 예약된 저장이 있다면 취소 (즉시 저장이 우선순위 높음)
        if (_saveCoroutine != null)
        {
            StopCoroutine(_saveCoroutine);
            _saveCoroutine = null;
        }

        _isSaving = true;

        // 로딩바 표시
        ShowLoadingUI(true);
        ExecuteSave(showLoading: true);
    }

    private void ExecuteSave(bool showLoading)
    {
        string json = JsonConvert.SerializeObject(userData);
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "PlayerStats", json } }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result =>
            {
                _isSaving = false;
                _retryCount = 0;
                if (showLoading) ShowLoadingUI(false);
                Debug.Log("[PlayFab] 데이터 저장 완료!");
            },
            error =>
            {
                if (_retryCount < MAX_RETRY)
                {
                    _retryCount++;
                    Debug.LogWarning($"저장 실패. 재시도 중... ({_retryCount}/{MAX_RETRY})");
                    Invoke(nameof(RetryExecuteSave), 2.0f);
                }
                else
                {
                    _isSaving = false;
                    _retryCount = 0;
                    if (showLoading) ShowLoadingUI(false);
                    Debug.LogError($"최종 저장 실패: {error.GenerateErrorReport()}");
                }
            }
        );
    }

    // Invoke용 헬퍼 함수
    private void RetryExecuteSave() => ExecuteSave(showLoading: _isSaving);

    private void ShowLoadingUI(bool show)
    {
        if (_loadingPanel == null && GlobalCanvas.Instance != null)
            _loadingPanel = GlobalCanvas.Instance.LoadingPanel;

        if (_loadingPanel != null)
        {
            if (show) _loadingPanel.gameObject.SetActive(true);
            else _loadingPanel.HideLoading();
        }
    }

    private void SaveDataImmediate()
    {
        string json = JsonConvert.SerializeObject(userData);
        var request = new UpdateUserDataRequest { Data = new Dictionary<string, string> { { "PlayerStats", json } } };
        PlayFabClientAPI.UpdateUserData(request, null, null);
    }
    #endregion

    #region Gameplay Logic
    public void OnGachaResult(TowerType rewardType)
    {
        if (HasTower(rewardType)) AddTowerExp(rewardType, 100);
        else userData._towers.Add(new TowerGameData(rewardType, 1, 0));

        SaveData(); // 가챠는 즉시 저장
    }

    private bool HasTower(TowerType type) => userData._towers.Exists(t => t._id == type);

    public void AddTowerExp(TowerType type, int amount)
    {
        if (userData?._towers == null) return;
        TowerGameData target = userData._towers.Find(t => t._id == type);
        if (target != null)
        {
            target._currentExp += amount;
            SaveData();
        }
    }

    public void RequestDeleteAccount()
    {
        userData._isDeleted = true;
        userData._lastLoginId = "";
        SaveDataImmediate();
        PlayerPrefs.SetString("CurrentCustomID", SystemInfo.deviceUniqueIdentifier + "_" + DateTime.Now.Ticks);
        PlayerPrefs.Save();
        var scriptReq = new ExecuteCloudScriptRequest { FunctionName = "DeletePlayerAccount" };
        PlayFabClientAPI.ExecuteCloudScript(scriptReq, r => StartCoroutine(QuitApplicationAfterDelay(1.5f)), null);
    }
    #endregion

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
#endregion