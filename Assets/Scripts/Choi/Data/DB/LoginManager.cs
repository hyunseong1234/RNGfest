using TMPro;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _uidText;
    [SerializeField] private GameObject _loginPanel;


    private void Start()
    {
        // 서버 매니저의 로그인 성공 이벤트에 내 함수를 등록
        PlayFabDataManager.Instance.OnLoginSuccessEvent += HandleLoginSuccess;

        // 시작하자마자 기존 계정이 있는지 찔러봄
        PlayFabDataManager.Instance.CheckExistingAccount();
    }

    // [버튼 연결용] 게스트 로그인 버튼 누를 때
    public void _OnClickGuestLogin()
    {
        PlayFabDataManager.Instance.SignUpGuest();
    }

    // 실제로 로그인이 성공했을 때 화면 처리
    private void HandleLoginSuccess()
    {
        if (_loginPanel != null) _loginPanel.SetActive(false);
        if (_uidText != null) _uidText.text = "UID: " + PlayFabDataManager.Instance.myPlayFabID;

        Debug.Log("로그인 성공으로 인해 UI를 닫습니다.");
        // 여기서 로비 씬 전환 등의 로직 추가 가능
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위해 이벤트 해제
        if (PlayFabDataManager.Instance != null)
            PlayFabDataManager.Instance.OnLoginSuccessEvent -= HandleLoginSuccess;
    }
}