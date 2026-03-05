using System.Collections;
using TMPro;
using UnityEngine;

namespace Dev.jeon.UI
{
    public class WavePopupUI : MonoBehaviour
    {
        [SerializeField] private GameObject _popupObject; // 팝업 부모 오브젝트
        [SerializeField] private TextMeshProUGUI _popupText;
       // [SerializeField] private Animator _animator; // 애니메이션 쓸 거면 연결

        public void PlayPopup(int waveIndex, WaveType type)
        {
            StopAllCoroutines(); // 이전 팝업이 실행 중이면 중지
            StartCoroutine(PopupRoutine(waveIndex, type));
        }

        private IEnumerator PopupRoutine(int waveIndex, WaveType type)
        {
            // 1. 텍스트 설정
            if (type == WaveType.Boss)
            {
                _popupText.text = "BOSS";
                _popupText.color = Color.red;
            }
            else
            {
                _popupText.text = $"WAVE {waveIndex}";
                _popupText.color = Color.white;
            }

            // 2. 팝업 활성화 및 애니메이션 실행
            //_popupObject.SetActive(true);
            //if (_animator != null) _animator.SetTrigger("Show");

            // 3. 2초 대기
            yield return new WaitForSeconds(2.0f);

            // 4. 팝업 비활성화
            _popupObject.SetActive(false);
        }
    }
}