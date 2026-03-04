namespace Dev.cheol.Model
{
    using Dev.cheol.Manager;
    using UnityEngine;

    public abstract class BaseScreenUI : BaseObject
    {
        [SerializeField] protected RectTransform rectTransform;

        [Header("Display Settings")]
        public Vector3 offset = new Vector3(0, 2.0f, 0);
        [SerializeField] protected float maxVisibleDistance = 50f;

        protected Camera mainCamera;

        protected override void Awake()
        {
            base.Awake();
            IsUI = true;
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        }

        protected virtual void Start()
        {
            RefreshCamera();
        }

        // 카메라가 바뀔 수도 있으니 별도 함수로 분리
        protected void RefreshCamera()
        {
            var camManager = ServiceLocator.Instance.GetService<CameraManager>();
            if (camManager != null && camManager.Camera != null)
            {
                mainCamera = camManager.Camera;
            }
            else
            {
                // 최후의 수단이지만, 매 프레임 호출하는 것보다 Start에서 한 번 하는 건 괜찮음
                mainCamera = Camera.main;
            }
        }

        public override void ObjectUpdate()
        {
            // 월드 좌표 계산 (부모가 공통으로 처리)
            if (_target == null || mainCamera == null) return;

            Vector3 worldPos = _target.position + offset;
            float distance = Vector3.Distance(mainCamera.transform.position, worldPos);

            // 카메라 뒤에 있거나 너무 멀면 연출 생략 (성능 최적화)
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            if (screenPos.z < 0 || distance > maxVisibleDistance)
            {
                rectTransform.localScale = Vector3.zero;
                return;
            }

            ApplyScreenPosition(screenPos, distance);
        }

        protected virtual void ApplyScreenPosition(Vector3 screenPos, float distance)
        {
            // 기본은 정직한 추적
            rectTransform.position = screenPos;
        }

        /// <summary>
        /// 통통 튀는 포물선 좌표를 계산해주는 함수 (X축 랜덤성 포함)
        /// </summary>
        /// <param name="time">경과 시간</param>
        /// <param name="jumpHeight">튀어오르는 높이</param>
        /// <param name="speed">속도</param>
        /// <param name="randomSide">옆으로 튀는 정도</param>
        protected Vector3 GetBounceOffset(float time, float jumpHeight, float speed, float randomSide)
        {
            // Y축: 포물선 운동 (시간에 따른 높이 변화)
            // h = v0*t - 0.5*g*t^2 식을 단순화한 형태
            float y = Mathf.Max(0, (jumpHeight * speed * time) - (0.5f * 9.81f * Mathf.Pow(speed * time, 2)));

            // X축: 일정한 속도로 옆으로 이동
            float x = randomSide * time;

            return new Vector3(x, y * 100f, 0); // UI 좌표계에 맞춰 높이 보정
        }
    }
}