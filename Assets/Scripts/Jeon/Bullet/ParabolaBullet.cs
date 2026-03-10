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
            // 타겟이 없으면 현재 위치로 설정해서 제자리 폭발 방지
            Vector3 lastTargetPos = _target != null ? _target.position : transform.position;

            float distance = Vector3.Distance(startPos, lastTargetPos);

            // [안전장치] 거리가 너무 짧으면 바로 피격 판정 (제자리 맴돌기 방지)
            if (distance < 0.1f)
            {
                HitTarget();
                yield break;
            }

            float totalTime = distance / _speed;
            float elapsedTime = 0f;

            while (elapsedTime < totalTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / totalTime; // 0 ~ 1 비율

                if (_target != null && _target.gameObject.activeSelf)
                    lastTargetPos = _target.position;

                // 1. 현재 프레임의 위치 계산
                Vector3 currentPos = Vector3.Lerp(startPos, lastTargetPos, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * _arcHeight;

                // 2. [핵심] 아주 살짝 미래(+0.05초)의 위치 계산
                float nextT = Mathf.Clamp01((elapsedTime + 0.05f) / totalTime);
                Vector3 nextPos = Vector3.Lerp(startPos, lastTargetPos, nextT);
                nextPos.y += Mathf.Sin(nextT * Mathf.PI) * _arcHeight;

                // 3. 현재 위치에서 미래 위치를 바라보게 회전 (절대 꼬이지 않음!)
                Vector3 moveDirection = nextPos - currentPos;
                if (moveDirection != Vector3.zero && moveDirection.sqrMagnitude > 0.001f)
                {
                    //transform.rotation = Quaternion.LookRotation(moveDirection);
                    transform.up = moveDirection;
                }

                // 4. 위치 이동
                transform.position = currentPos;
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