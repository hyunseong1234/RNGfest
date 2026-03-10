using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
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
    }

    public static PlayFabDataManager Instance;

    [Header("서버 설정")]
    public string titleId = "18F60C";

    [Header("유저 데이터")]
    public string myPlayFabID;
    public UserGameData userData;

    // 로그인 성공 시 실행될 콜백 이벤트
    public Action OnLoginSuccessEvent;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        PlayFabSettings.staticSettings.TitleId = titleId;
    }

    // 1. 기존 계정 확인 (자동 로그인 시도)
    public void CheckExistingAccount()
    {
        Debug.Log("기존 계정 확인 중...");
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = false
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, error =>
        {
            if (error.Error == PlayFabErrorCode.AccountNotFound)
                Debug.Log("신규 유저입니다. 로그인이 필요합니다.");
            else
                Debug.LogError("서버 오류: " + error.GenerateErrorReport());
        });
    }

    // 2. 신규 게스트 가입 버튼 클릭 시 호출
    public void SignUpGuest()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        myPlayFabID = result.PlayFabId;

        if (result.NewlyCreated)
        {
            Debug.Log($"[신규 유저] UID({myPlayFabID}) 생성.");
            InitializeNewUser();
        }
        else
        {
            Debug.Log($"[기존 유저] UID({myPlayFabID}) 로그인.");
            LoadData();
        }

        // 로그인이 완전히 끝났음을 UI 매니저 등에 알림
        OnLoginSuccessEvent?.Invoke();
    }

    private void OnLoginFailure(PlayFabError error) => Debug.LogError("접속 실패: " + error.GenerateErrorReport());

    private void InitializeNewUser()
    {
        userData = new UserGameData();
        SaveData();
    }

    public void SaveData()
    {
        if (userData == null) return;
        string json = JsonConvert.SerializeObject(userData);
        var request = new UpdateUserDataRequest { Data = new Dictionary<string, string> { { "PlayerStats", json } } };
        PlayFabClientAPI.UpdateUserData(request, result => Debug.Log("저장 성공"), OnLoginFailure);
    }

    public void LoadData()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerStats"))
            {
                userData = JsonConvert.DeserializeObject<UserGameData>(result.Data["PlayerStats"].Value);
                Debug.Log("데이터 로드 완료");
            }
            else
            {
                InitializeNewUser();
            }
        }, OnLoginFailure);
    }
}