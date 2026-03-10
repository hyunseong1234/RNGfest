using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

// 서버에 보낼 데이터 구조 (기획 내용 반영)
[System.Serializable]
public class UserGameData
{
    public int gold = 100;
    public int high_score = 0;
    public List<int> towerLevels = new List<int> { 1, 1, 1, 1, 1 }; // 초기 타워 레벨들
}

public class PlayFabDataManager : MonoBehaviour
{
    public static PlayFabDataManager Instance;

    [Header("Settings")]
    public string titleId = "18F60C";
    public UserGameData userData; // 현재 게임 중인 데이터

    void Awake()
    {
        Instance = this;
        // 서버 주소 고정
        PlayFabSettings.staticSettings.TitleId = titleId;
    }

    void Start()
    {
        Login();
    }

    // [1] 로그인: 기기 고유 번호로 자동 계정 생성
    public void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result =>
            {
                Debug.Log("서버 로그인 성공!");
                LoadData(); // 로그인 성공하면 바로 데이터 불러오기
            },
            error => Debug.LogError("로그인 실패: " + error.GenerateErrorReport()));
    }

    // [2] 데이터 적재 (Save): 클라우드 서버로 전송
    public void SaveData()
    {
        if (userData == null) return;

        // 객체를 JSON 문자열로 변환 (Json.NET 사용)
        string json = JsonConvert.SerializeObject(userData);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "PlayerStats", json } }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log("클라우드 저장 완료!"),
            error => Debug.LogError("저장 실패: " + error.GenerateErrorReport()));
    }

    // [3] 데이터 불러오기 (Load): 서버에서 가져오기
    public void LoadData()
    {
        var request = new GetUserDataRequest();

        PlayFabClientAPI.GetUserData(request, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerStats"))
            {
                // 서버 JSON을 다시 객체로 복구
                string json = result.Data["PlayerStats"].Value;
                userData = JsonConvert.DeserializeObject<UserGameData>(json);
                Debug.Log("기존 데이터 불러오기 성공!");
            }
            else
            {
                Debug.Log("신규 유저입니다. 초기 데이터를 설정합니다.");
                userData = new UserGameData(); // 새 데이터 생성
                SaveData(); // 서버에 첫 기록 생성
            }
        }, error => Debug.LogError("불러오기 실패: " + error.GenerateErrorReport()));
    }
}