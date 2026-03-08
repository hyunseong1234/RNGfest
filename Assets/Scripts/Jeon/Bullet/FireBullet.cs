using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class FireBullet : BaseBullet
    {
        [Header("화염 스플래시 설정")]
        [SerializeField] private float _splashRadius = 1.5f;

        [Header("시각 효과")]
        [SerializeField] private BaseObject _explosionEffectPrefab;
        // _effectDuration은 이제 이펙트 스크립트가 관리하므로 여기서 지워도 됩니다.

        private Coroutine _moveCoroutine;

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
            // 1. 처음 조준했던 타겟의 위치를 일단 기억
            Vector3 lastTargetPos = _target.position;

            // 2. 도착할 때까지 멈추지 않는 루프
            while (true)
            {
                // 타겟이 아직 살아있다면 실시간으로 위치를 계속 갱신 (유도탄)
                if (_target != null && _target.gameObject.activeSelf)
                {
                    lastTargetPos = _target.position;
                }

                // 3. '마지막으로 확인된 위치'를 향해 계속 이동
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    lastTargetPos,
                    _speed * Time.deltaTime
                );

                // 4. 드디어 목적지(마지막 위치)에 도달했다면?
                if (Vector3.Distance(transform.position, lastTargetPos) < 0.05f)
                {
                    // 여기서 중요! 타겟이 있든 없든 '이 좌표'에서 폭발을 일으킵니다.
                    Explode(lastTargetPos);
                    yield break; // 코루틴 종료
                }

                yield return null;
            }
        }

        private void Explode(Vector3 explosionCenter)
        {
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();
            var poolManager = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            // 1. 데미지 처리
            if (mainManager != null)
            {
                float sqrRadius = _splashRadius * _splashRadius;
                var enemiesInRange = mainManager.SpawnEnemys
                    .Where(e => e != null && e.gameObject.activeSelf)
                    .Where(e => (e.transform.position - explosionCenter).sqrMagnitude <= sqrRadius)
                    .ToList();

                foreach (var enemy in enemiesInRange)
                {
                    enemy.OnDamaged(_damage, _fontColor);
                }
            }

            // 2. 이펙트 생성 (생성만 하고 신경 끕니다)
            if (poolManager != null && _explosionEffectPrefab != null)
            {
                var explosionEffect = poolManager.GetFromPool<BaseObject>(_explosionEffectPrefab);
                if (explosionEffect != null)
                {
                    explosionEffect.transform.position = explosionCenter + new Vector3(0, 0.5f, 0);
                }
            }

            // 3. 총알은 즉시 퇴근!
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

        public override void ObjectUpdate() { }
    }
}