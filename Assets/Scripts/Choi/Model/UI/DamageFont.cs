using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Dev.cheol.Model
{
    // 형님 요청대로 맛깔나는 색상 10종 + @로 구성했습니다.
    public enum FontColor
    {
        White,      // 흰색 (기본)
        Yellow,     // 노랑 (크리티컬 등)
        Black,      // 블랙 (암흑 등)
        Cyan,       // 카얀 (빙결/마나 등)
        Green,      // 연두 (회복/독 등)
        Pink,       // 분홍 (러블리?)
        Red,        // 빨강 (강력한 데미지)
        Orange,     // 주황 (화상)
        Purple,     // 보라 (저주)
        Blue,       // 파랑 (마법)
        Gold,       // 금색 (전설급)
        Max
    }

    public class DamageFont : BaseScreenUI
    {
        [SerializeField] private TextMeshProUGUI _damageText;

        [Space(10)]
        [SerializeField] private float _lifeTime = 0.7f;

        [Header("폰트 크기")]
        [SerializeField] private float _minScale = 0.01f;
        [SerializeField] private float _maxScale = 0.02f;
        [SerializeField] private float _popStrength = 0.005f;
        [SerializeField] private float _referenceDistance = 12f;

        [Header("점프 크기")]
        [SerializeField] private float _jumpHeight = 1.0f;
        [SerializeField] private float _bounceSpeed = 3.0f;
        [SerializeField] private float _sideForceRange = 1.2f;

        private Coroutine _animCoroutine;

        public override void OnSpawn()
        {
            base.OnSpawn();
        }

        // 색상 매칭 함수: colorNum(int)이 들어오면 FontColor enum에 맞춰 색상을 반환합니다.
        private Color GetColor(FontColor colorNum)
        {
            FontColor colorType = colorNum;

            return colorType switch
            {
                FontColor.White => Color.white,
                FontColor.Yellow => Color.yellow,
                FontColor.Black => Color.black,
                FontColor.Cyan => Color.cyan,
                FontColor.Green => new Color(0.2f, 1f, 0.2f), // 맛깔나는 연두
                FontColor.Pink => new Color(1f, 0.4f, 0.7f), // 핫핑크
                FontColor.Red => Color.red,
                FontColor.Orange => new Color(1f, 0.6f, 0f),   // 쨍한 주황
                FontColor.Purple => new Color(0.6f, 0.2f, 1f), // 진보라
                FontColor.Blue => Color.blue,
                FontColor.Gold => new Color(1f, 0.84f, 0f),  // 골드
                _ => Color.white                // 기본 흰색
            };
        }

        public void SetDamage(float amount, Transform targetUnit, FontColor colorType)
        {
            // 1. 데이터 및 색상 세팅
            if (_damageText != null)
            {
                int finalDamage = Mathf.RoundToInt(amount);
                _damageText.text = finalDamage.ToString();
                _damageText.color = GetColor(colorType); // 여기서 색상 변경!
            }
            _target = targetUnit;

            // 2. 청소 후 연출 시작
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);

            RefreshCamera();
            _animCoroutine = StartCoroutine(Co_PlayAnimation());
        }

        private IEnumerator Co_PlayAnimation()
        {
            while (mainCamera == null)
            {
                RefreshCamera();
                if (mainCamera == null) yield return null;
            }

            float elapsed = 0f;
            float sideForce = Random.Range(-_sideForceRange, _sideForceRange);
            transform.localScale = Vector3.zero;

            while (elapsed < _lifeTime)
            {
                if (_target == null || !gameObject.activeInHierarchy) break;

                elapsed += Time.deltaTime;

                // 1. 위치 추적 및 빌보드
                transform.position = _target.position + offset;
                transform.rotation = mainCamera.transform.rotation;

                // 2. 통통 튀는 연출
                transform.position += GetBounceOffset(elapsed, _jumpHeight, _bounceSpeed, sideForce);

                // 3. 거리 기반 스케일링
                Vector3 diff = mainCamera.transform.position - transform.position;
                float dist = Mathf.Sqrt(diff.sqrMagnitude);

                float baseScale = Mathf.Clamp(_referenceDistance / (dist + 0.01f), _minScale, _maxScale);
                float pop = Mathf.Sin(Mathf.Clamp01(elapsed * 8f) * Mathf.PI);

                transform.localScale = Vector3.one * (baseScale + (pop * _popStrength));

                yield return null;
            }

            FinalizeAndReturn();
        }

        private void FinalizeAndReturn()
        {
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = null;
            _target = null;

            var main = ServiceLocator.Instance.GetService<MainManager>();
            if (main != null && main.SpawnUI.Contains(this))
            {
                main.SpawnUI.Remove(this);
            }

            var poolManager = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            if (poolManager != null)
            {
                poolManager.ReturnPool(this);
            }
        }
    }
}