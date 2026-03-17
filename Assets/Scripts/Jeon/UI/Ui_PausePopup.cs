using UnityEngine;
using Dev.cheol.Manager;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수

namespace Dev.jeon.UI
{
    public class Ui_PausePopup : UIObject
    {
        [SerializeField] private GameObject _settingSubPanel;
        [SerializeField] private GameObject _pausePanel;

        public void ClosePauseMenu()
        {
            Time.timeScale = 1f;
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(false);
            }
        }

        // 기존 Replay 대신 Lobby로 이동하는 기능으로 변경
        public void OnClick_Lobby()
        {
            // 1. 멈췄던 시간을 다시 정상으로 돌립니다. (매우 중요!)
            Time.timeScale = 1f;

            // 2. "Game lobby" 씬을 로드합니다.
            // 주의: 빌드 설정(Build Settings)에 해당 씬이 등록되어 있어야 합니다.
            SceneManager.LoadScene("Game lobby");
        }

        public void OnClick_Settings()
        {
            if (_settingSubPanel != null)
            {
                _settingSubPanel.SetActive(true);
            }
        }
    }
}