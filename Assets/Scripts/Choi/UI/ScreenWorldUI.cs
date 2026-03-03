using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Dev.cheol.UI
{
    public class ScreenWorldUI : MonoBehaviour
    {
        [SerializeField] protected Transform _target;
        public Vector3 offset = new Vector3(0, 2.0f, 0); // 유닛 머리 위 높이 조절

        [SerializeField] private RectTransform rectTransform;
        private Camera mainCamera;

        public Transform Target { get => _target; set => _target = value; }


        protected virtual void OnEnable()
        {
            if (rectTransform == null) rectTransform = transform.GetComponent<RectTransform>();
        }
        // Update is called once per frame
        void LateUpdate()
        {
            //잠시만 테스트
            Test();
        }

        private void Test()
        {
            if (_target == null) return;
            if (mainCamera == null) mainCamera = Camera.main;

            // 1. 유닛의 월드 좌표 + 오프셋
            Vector3 worldPos = _target.position + offset;

            // 2. 카메라와의 거리 계산 (이게 핵심!)
            float distance = Vector3.Distance(mainCamera.transform.position, worldPos);

            // 3. 월드 좌표를 스크린 좌표로 변환
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

            // 카메라 뒤에 있거나 너무 멀면 숨기기
            if (screenPos.z < 0 || distance > 50f) // 50f는 적절한 최대 가시거리
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            // 4. 위치 적용
            rectTransform.position = screenPos;

            // 기준 거리(예: 10m)에서 크기가 1이 되도록 설정
            float baseDistance = 15f;
            float minScale = 0.2f; // 너무 작아지지 않게
            float maxScale = 0.5f; // 너무 커지지 않게

            float scale = baseDistance / distance;
            rectTransform.localScale = Vector3.one * Mathf.Clamp(scale, minScale, maxScale);
        }

        void SetVisible(bool visible)
        {
            // CanvasGroup이나 GameObject 활성화를 통해 가리기
            if (gameObject.activeSelf != visible)
                gameObject.SetActive(visible);
        }
    }

}

