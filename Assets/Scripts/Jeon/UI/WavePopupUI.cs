using System.Collections;
using TMPro;
using UnityEngine;

namespace Dev.jeon.UI
{
    public class WavePopupUI : MonoBehaviour
    {
        [Header("UI 내용물 (자식 오브젝트)")]
        [SerializeField] private GameObject _popupObject; // 실제 UI가 담긴 자식
        [SerializeField] private TextMeshProUGUI _popupText;

        [Header("보스전 연출")]
        [SerializeField] private CanvasGroup _dangerOverlay;

        public void PlayPopup(int waveIndex, WaveType type)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            // 이제 이 스크립트가 붙은 오브젝트가 켜져 있으므로 코루틴 실행 가능!
            StopAllCoroutines();
            StartCoroutine(PopupRoutine(waveIndex, type));
        }

        private IEnumerator PopupRoutine(int waveIndex, WaveType type)
        {
            //  알파 0 초기화
            if (_dangerOverlay != null)
            {
                _dangerOverlay.alpha = 0;
            }

            // 2. 웨이브 타입에 따른 텍스트 설정
            if (type == WaveType.Boss)
            {
                _popupText.text = "DANGER";
                _popupText.color = Color.red;

                if (_dangerOverlay != null) StartCoroutine(FlashDangerScreen());
            }
            else
            {
                _popupText.text = $"WAVE {waveIndex}";
                _popupText.color = Color.white;
            }

            // 본체(this.gameObject)가 아니라 자식 내용물만 켬
            _popupObject.SetActive(true);

            yield return new WaitForSeconds(2.0f);

            _popupObject.SetActive(false);
            if (_dangerOverlay != null) _dangerOverlay.alpha = 0;
        }

        private IEnumerator FlashDangerScreen()
        {
            //깜빡이 로직
            for (int i = 0; i < 3; i++)
            {
                float timer = 0;
                while (timer < 0.2f)
                {
                    timer += Time.deltaTime;
                    _dangerOverlay.alpha = Mathf.Lerp(0, 0.4f, timer / 0.2f);
                    yield return null;
                }
                timer = 0;
                while (timer < 0.2f)
                {
                    timer += Time.deltaTime;
                    _dangerOverlay.alpha = Mathf.Lerp(0.4f, 0, timer / 0.2f);
                    yield return null;
                }
            }
        }
    }
}