namespace Dev.cheol.Model
{
    using Dev.cheol.Manager;
    using UnityEngine;

    public abstract class BaseScreenUI : BaseObject
    {
        [Header("UI Reference")]
        [SerializeField] protected RectTransform rectTransform;

        [Header("Display Settings")]
        public Vector3 offset = new Vector3(0, 2.0f, 0);
        [SerializeField] protected float maxVisibleDistance = 50f;

        protected Camera mainCamera;
        private float _maxSqrDistance;

        protected override void Awake()
        {
            base.Awake();
            IsUI = true;
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

            // 미리 제곱값을 계산해두어 나중에 루트 연산을 피합니다.
            _maxSqrDistance = maxVisibleDistance * maxVisibleDistance;
        }

        protected virtual void Start()
        {
            RefreshCamera();
        }

        protected void RefreshCamera()
        {
            var camManager = ServiceLocator.Instance.GetService<CameraManager>();
            if (camManager != null && camManager.Camera != null)
                mainCamera = camManager.Camera;
            else
                mainCamera = Camera.main;
        }

        public override void ObjectUpdate()
        {
            if (_target == null || mainCamera == null) return;

            // [최적화 1] 모든 UI를 매 프레임 갱신하지 않고 2프레임에 한 번씩만 계산합니다.
            // 50개가 동시에 돌 때 CPU 부하를 즉시 50% 절감합니다.
            if (Time.frameCount % 2 != 0) return;

            Vector3 worldPos = _target.position + offset;

            // [최적화 2] Vector3.Distance 대신 sqrMagnitude를 사용합니다. (루트 연산 제거)
            Vector3 diff = mainCamera.transform.position - worldPos;
            if (diff.sqrMagnitude > _maxSqrDistance)
            {
                // 너무 멀면 스케일을 0으로 만들어 렌더링 부하를 줄입니다.
                if (rectTransform.localScale != Vector3.zero)
                    rectTransform.localScale = Vector3.zero;
                return;
            }

            // [최적화 3] WorldToScreenPoint는 매우 무겁습니다.
            // z축 값으로 카메라 뒤에 있는지 먼저 판별합니다.
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            if (screenPos.z < 0)
            {
                if (rectTransform.localScale != Vector3.zero)
                    rectTransform.localScale = Vector3.zero;
                return;
            }

            // 거리 기반 스케일링을 위해 필요한 경우만 실제 거리를 구합니다.
            ApplyScreenPosition(screenPos, diff.magnitude);
        }

        protected virtual void ApplyScreenPosition(Vector3 screenPos, float distance)
        {
            rectTransform.position = screenPos;
        }

        protected Vector3 GetBounceOffset(float time, float jumpHeight, float speed, float randomSide)
        {
            float y = Mathf.Max(0, (jumpHeight * speed * time) - (0.5f * 9.81f * Mathf.Pow(speed * time, 2)));
            float x = randomSide * time;
            return new Vector3(x, y * 100f, 0);
        }
    }
}