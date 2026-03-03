using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class ParabolaBullet : BaseBullet
    {
        private float _speed = 20f;
        [SerializeField] private int _damage = 10;
        private Coroutine _moveCoroutine;

        [Header("포물선 설정")]
        [Tooltip("포물선의 최대 솟구치는 높이")]
        [SerializeField] private float _arcHeight = 5f;

        public override void Init(Transform target, int damage, float speed = 20f)
        {
            _target = target;
            _damage = damage;
            _speed = speed;

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = StartCoroutine(MoveToTarget());
            }
        }

        private IEnumerator MoveToTarget()
        {
            Vector3 startPos = transform.position;

            // 타겟까지의 거리와 도달하는 데 걸리는 총 시간 계산
            float distance = Vector3.Distance(startPos, _target.position);
            float totalTime = distance / _speed;
            float elapsedTime = 0f;

            // 타겟이 살아있는 동안 & 정해진 시간 동안 이동
            while (elapsedTime < totalTime && _target != null && _target.gameObject.activeSelf)
            {
                elapsedTime += Time.deltaTime;

                // 진행률 t (0에서 시작해 1로 끝남)
                float t = elapsedTime / totalTime;

                // 1. 시작점부터 타겟의 '현재 위치'까지 평면적인(직선) 이동 위치 계산
                Vector3 currentPos = Vector3.Lerp(startPos, _target.position, t);

                // 2. 포물선 높이 계산 (Sin 곡선 활용)
                // t가 0일 때 0, 0.5일 때 1, 1일 때 0이 되는 완벽한 아치형 곡선
                float heightOffset = Mathf.Sin(t * Mathf.PI) * _arcHeight;

                // 3. 평면 이동 위치에 높이(Y값) 더하기
                currentPos.y += heightOffset;
                
                // 총알이 날아가는 방향을 자연스럽게 바라보게 회전 (선택 사항)
                Vector3 moveDirection = currentPos - transform.position;
                if (moveDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(moveDirection);
                }

                transform.position = currentPos;

                yield return null;
            }

            // 루프가 끝났을 때 타겟이 여전히 살아있다면 명중 처리
            if (_target != null && _target.gameObject.activeSelf)
            {
                HitTarget();
            }
            else
            {
                // 날아가는 도중 몬스터가 죽었다면 그냥 사라짐
                ReturnToPool();
            }
        }

        private void HitTarget()
        {
            var enemy = _target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnDamaged(_damage);
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }

        private void OnDisable()
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
            throw new System.NotImplementedException();
        }
    }
}