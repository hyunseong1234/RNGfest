using UnityEngine;
using Dev.cheol.Manager;
using UnityEngine.SceneManagement; // 리플레이(씬 재시작) 기능용

namespace Dev.jeon.UI
{
    public class Ui_PauseButton : UIObject
    {
        [SerializeField] private GameObject _pausePopupPanel; // 꺼져있는 팝업창 프리팹/오브젝트 연결

        public void OpenPauseMenu()
        {
            if (_pausePopupPanel != null)
            {
                // 꺼져있는 오브젝트를 여기서 켜준다!
                _pausePopupPanel.SetActive(true);
            }
        }
    }
}