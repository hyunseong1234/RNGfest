using System.Collections;
using UnityEngine;
using Dev.cheol.Model;

namespace Dev.jeon.Effect
{
    public class TargetScopeEffect : BaseObject
    {
        [Header("3D 스코프 설정")]
        [SerializeField] private float _startScale = 3f;
        [SerializeField] private float _endScale = 1f;
        [SerializeField] private float _rotateSpeed = 180f;

        [Header("3D 머티리얼 설정")]
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Color _startColor = Color.yellow;
        [SerializeField] private Color _endColor = Color.red;

        private Coroutine _lockOnCoroutine;

        public void StartLockOn(float duration)
        {
            //  1. 이전에 돌고 있던 코루틴이 있다면 확실히 정지 (풀링 재사용 시 필수)
            if (_lockOnCoroutine != null)
            {
                StopCoroutine(_lockOnCoroutine);
            }

            gameObject.SetActive(true);
            _lockOnCoroutine = StartCoroutine(LockOnRoutine(duration));
        }

        private IEnumerator LockOnRoutine(float duration)
        {
            float elapsedTime = 0f;

            //  2. 시작 시 초기 상태로 강제 리셋 (이전 데이터 삭제)
            transform.localScale = Vector3.one * _startScale;
            if (_meshRenderer != null)
            {
                // material.color는 인스턴스를 생성하므로, 
                // 재사용 시 메모리 최적화를 위해 sharedMaterial을 쓰거나 캐싱하는 게 좋지만 
                // 일단 색상 리셋을 확실히 합니다.
                _meshRenderer.material.color = _startColor;
            }

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                transform.localScale = Vector3.one * Mathf.Lerp(_startScale, _endScale, t);
                transform.Rotate(Vector3.forward * _rotateSpeed * Time.deltaTime);

                if (_meshRenderer != null)
                {
                    _meshRenderer.material.color = Color.Lerp(_startColor, _endColor, t);
                }
                yield return null;
            }

            // 조준 완료 후 마지막 상태 고정
            transform.localScale = Vector3.one * _endScale;
            if (_meshRenderer != null) _meshRenderer.material.color = _endColor;

            _lockOnCoroutine = null;
        }

        //  3. 풀링 시스템에서 반납될 때 호출되는 함수 (BaseObject에 있다면 override)
        // 만약 BaseObject에 OnReturnToPool이 없다면 OnDisable이라도 활용해야 합니다.
        public virtual void OnReturnToPool()
        {
            if (_lockOnCoroutine != null)
            {
                StopCoroutine(_lockOnCoroutine);
                _lockOnCoroutine = null;
            }

            // 상태 초기화
            transform.localScale = Vector3.one * _startScale;
            gameObject.SetActive(false);
        }

        public override void ObjectUpdate() { }
    }
}