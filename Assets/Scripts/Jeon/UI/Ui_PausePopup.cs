using UnityEngine;
using Dev.cheol.Manager;
using UnityEngine.SceneManagement;

namespace Dev.jeon.UI
{
    // 이름을 Popup으로 바꿔서 역할을 명확히 하자!
    public class Ui_PausePopup : UIObject
    {
        [SerializeField] private GameObject _settingSubPanel;

        // 이 창이 활성화될 때 자동으로 게임을 멈추게 하면 편해!
        // [중요] 인스펙터에서 진짜 꺼야 할 'Setting_Image' 패널을 여기에 드래그하세요!
        [SerializeField] private GameObject _pausePanel;

       
        public void ClosePauseMenu()
        {
            Time.timeScale = 1f;

            // gameObject.SetActive(false) 대신, 연결된 패널을 직접 끕니다.
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(false);
                Debug.Log("패널 꺼짐 확인");
            }
            else
            {
                // 인스펙터에 연결 안 했을 때를 위한 경고
                Debug.LogError("_pausePanel이 연결되지 않았습니다!");
            }
        }

        public void OnClick_Replay()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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