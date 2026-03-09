using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using UnityEngine;

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
            // 발사 시점의 타겟 위치 저장 (타겟이 사라져도 그 지점까지는 가기 위함)
            Vector3 lastTargetPos = _target.position;

            float distance = Vector3.Distance(startPos, lastTargetPos);
            float totalTime = distance / _speed;
            float elapsedTime = 0f;

            // [수정] 타겟이 사라져도 끝까지 날아가도록 totalTime 기준으로만 체크
            while (elapsedTime < totalTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / totalTime;

                // 타겟이 살아있다면 실시간 위치 갱신, 죽었다면 마지막 위치 유지
                if (_target != null && _target.gameObject.activeSelf)
                    lastTargetPos = _target.position;

                Vector3 currentPos = Vector3.Lerp(startPos, lastTargetPos, t);
                float heightOffset = Mathf.Sin(t * Mathf.PI) * _arcHeight;
                currentPos.y += heightOffset;

                Vector3 moveDirection = currentPos - transform.position;
                if (moveDirection != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(moveDirection);

                transform.position = currentPos;
                yield return null;
            }

            // [수정] 도착했을 때의 판단은 HitTarget 하나로 통일합니다.
            HitTarget();
        }

        protected virtual void HitTarget()
        {
            // 타겟이 유효할 때만 데미지
            if (_target != null && _target.gameObject.activeSelf)
            {
                var enemy = _target.GetComponent<Enemy>();
                if (enemy != null) enemy.OnDamaged(_damage, _fontColor);
            }

            ReturnToPool();
        }

        // [핵심] private을 protected virtual로 바꿔야 SkillBullet이 에러 없이 오버라이드합니다.
        protected virtual void ReturnToPool()
        {
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            if (pool != null) pool.ReturnPool(this);
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

        public override void ObjectUpdate()
        {
        }
    }
}