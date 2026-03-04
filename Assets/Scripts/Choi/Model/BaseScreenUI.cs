namespace Dev.cheol.Model
{
    using Dev.cheol.Manager;
    using UnityEngine;

    public abstract class BaseScreenUI : BaseObject
    {
        [SerializeField] protected RectTransform rectTransform;

        [Header("Base UI Settings")]
        [Tooltip("몬스터 머리 위 오프셋 좌표입니다.")]
        [SerializeField] public Vector3 offset = new Vector3(0, 2.5f, 0);

        protected Camera mainCamera;

        protected override void Awake()
        {
            base.Awake();
            IsUI = true;
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            RefreshCamera();
        }

        protected virtual void Start()
        {
            RefreshCamera();
        }

        protected void RefreshCamera()
        {
            var camManager = ServiceLocator.Instance.GetService<CameraManager>();
            if (camManager != null && camManager.Camera != null)
            {
                mainCamera = camManager.Camera;
            }
            else
            {
                mainCamera = Camera.main;
            }
        }

        // 매니저 업데이트는 사용하지 않으므로 비워둡니다.
        public override void ObjectUpdate() { }

        /// <summary>
        /// 통통 튀는 포물선 좌표를 계산해주는 공통 함수
        /// </summary>
        protected Vector3 GetBounceOffset(float time, float jumpHeight, float speed, float randomSide)
        {
            // Y축: 포물선 운동 (h = v0*t - 0.5*g*t^2 식을 단순화)
            float y = Mathf.Max(0, (jumpHeight * speed * time) - (0.5f * 9.81f * Mathf.Pow(speed * time, 2)));

            // X축: 일정한 속도로 옆으로 이동
            float x = randomSide * time;

            // 월드 스페이스 좌표에 맞춰 반환
            return new Vector3(x, y, 0);
        }
    }
}