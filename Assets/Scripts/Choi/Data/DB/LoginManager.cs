using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _uidText;

    [SerializeField] private GameObject _signUpPanel;

    [SerializeField] private GameObject _startPanel;

    private void Start()
    {

        if (_signUpPanel != null) _signUpPanel.SetActive(false);


        PlayFabDataManager.Instance.OnLoginSuccessEvent += HandleLoginSuccess;
        PlayFabDataManager.Instance.OnNeedSignUpEvent += HandleNeedSignUp;

        // 3. 기존 유저인지 자동 로그인 시도
        PlayFabDataManager.Instance.CheckExistingAccount();
    }

    // 기존 유저가 없어서 가입이 필요할 때 호출됨
    private void HandleNeedSignUp()
    {
        Debug.Log("신규 유저용 가입 패널 활성화");
        if (_signUpPanel != null) _signUpPanel.SetActive(true);
    }

    // 로그인이 최종 성공했을 때 호출됨 (자동 로그인 혹은 가입 완료 후)
    private void HandleLoginSuccess()
    {
        Debug.Log("최종 로그인 성공: 패널을 닫고 UI를 갱신합니다.");
        if (_signUpPanel != null) _signUpPanel.SetActive(false);

        if (_uidText != null)
            _uidText.text = "UID: " + PlayFabDataManager.Instance.myPlayFabID;
        if (_startPanel != null)
            _startPanel.SetActive(true);
    }

    /// <summary>
    /// 게스트 가입 버튼에 연결
    /// </summary>
    public void _OnClickGuestSignUp()
    {
        PlayFabDataManager.Instance.SignUpNewAccount();
    }

    /// <summary>
    /// 탈퇴 버튼에 연결
    /// </summary>
    public void _OnClickDelete()
    {
        PlayFabDataManager.Instance.RequestDeleteAccount();
    }

    public void _OnClickNextScene()
    {
        SceneManager.LoadScene(1); //
    }

    private void OnDestroy()
    {
        if (PlayFabDataManager.Instance != null)
        {
            PlayFabDataManager.Instance.OnLoginSuccessEvent -= HandleLoginSuccess;
            PlayFabDataManager.Instance.OnNeedSignUpEvent -= HandleNeedSignUp;
        }
    }
}