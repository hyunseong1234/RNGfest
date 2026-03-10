using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Dev.jeon.Bullet
{
    public class ParabolaBullet : BaseBullet
    {
        private Coroutine _moveCoroutine;

        [Header("포물선 설정")]
        [SerializeField] protected float _arcHeight = 5f;

        public override void Init(Transform target, float damage, float speed = 20f)
        {
            _target = target;
            _damage = damage;
            _speed = speed;

            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveToTarget());
        }

        private IEnumerator MoveToTarget()
        {
            Vector3 startPos = transform.position;
            Vector3 lastTargetPos = _target != null ? _target.position : transform.position;

            float distance = Vector3.Distance(startPos, lastTargetPos);
            float totalTime = distance / _speed;
            float elapsedTime = 0f;

            while (elapsedTime < totalTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / totalTime; // 0 에서 1까지 증가

                if (_target != null && _target.gameObject.activeSelf)
                    lastTargetPos = _target.position;

                // 1. 현재 위치 계산 (기존과 동일)
                Vector3 currentPos = Vector3.Lerp(startPos, lastTargetPos, t);
                float heightOffset = Mathf.Sin(t * Mathf.PI) * _arcHeight;
                currentPos.y += heightOffset;
                transform.position = currentPos;

                // 2. [핵심] 완벽한 포물선 방향(접선) 계산
                // 목표를 향해 가는 기본 방향 벡터
                Vector3 forwardVector = lastTargetPos - startPos;

                // 코사인(Cos) 함수 미분을 이용해 정점 전에는 위를 보고, 정점 후에는 아래를 보게 함
                forwardVector.y += Mathf.Cos(t * Mathf.PI) * _arcHeight * Mathf.PI;

                // 3. 머리 회전 적용
                if (forwardVector != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(forwardVector);
                }

                yield return null;
            }

            HitTarget();
        }

        protected virtual void HitTarget()
        {
            // 1. 공통 히트 이펙트 재생 (BaseBullet에 구현한 함수)
            SpawnHitEffect(transform.position);


            if (_target != null && _target.gameObject.activeSelf)
            {
                if (_target.TryGetComponent(out Enemy enemy))
                {
                    enemy.OnDamaged(_damage, _fontColor);
                }
            }
            ReturnToPool();
        }

        // SkillBullet이 접근할 수 있도록 다시 살려둡니다.
        protected override void ReturnToPool()
        {
            base.ReturnToPool();
        }

        protected virtual void OnDisable()
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
            _target = null;
        }

        public override void ObjectUpdate() { }
    }
}