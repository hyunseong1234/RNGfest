using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Stats;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class FireBullet : BaseBullet
    {
        private float _speed = 20f;
        [SerializeField] private int _damage = 10;           // 화염 스플래시 데미지
        [SerializeField] private float _splashRadius = 1.5f; // 스플래시 범위 (반경 1.5)

        [Header("시각 효과")]
        [SerializeField] private BaseObject _explosionEffectPrefab;// 풀링에 등록할 폭발 파티클
        [SerializeField] private float _effectDuration = 0.5f; // 파티클 유지 시간

        private Coroutine _moveCoroutine;

        public override void Init(Transform target, int damage, float speed = 20f)
        {
            _target = target;
            _damage = damage;
            _speed = speed;

            // 기존에 돌던 코루틴이 있다면 방어적으로 중지
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveToTarget());
        }

        private IEnumerator MoveToTarget()
        {
            // 타겟이 살아있는 동안 계속 추적
            while (_target != null && _target.gameObject.activeSelf)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    _target.transform.position,
                    _speed * Time.deltaTime
                );

                if (Vector3.Distance(transform.position, _target.transform.position) < 0.05f)
                {
                    // 목표에 도달하면 파티클 폭발과 스플래시 데미지를 주는 코루틴으로 전환
                    StartCoroutine(HitAndExplode());
                    yield break;
                }

                yield return null;
            }

            // 날아가는 도중 타겟이 죽었다면 총알 조용히 반납
            ReturnToPool();
        }

        private IEnumerator HitAndExplode()
        {
            // 폭발 중심점 (타겟이 날아가는 도중 죽었을 수도 있으니, 현재 총알 위치를 중심으로 잡음)
            Vector3 explosionCenter = transform.position;
            if (_target != null) explosionCenter = _target.transform.position;

            var mainManager = ServiceLocator.Instance.GetService<MainManager>();
            var poolManager = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            // 1. 반경 1.5 이내의 모든 적 탐색 후 스플래시 데미지 적용
            if (mainManager != null)
            {
                float sqrRadius = _splashRadius * _splashRadius; // 최적화를 위해 제곱근 연산

                var enemiesInRange = mainManager.SpawnEnemys
                    .Where(e => e != null && e.gameObject.activeSelf)
                    .Where(e => (e.transform.position - explosionCenter).sqrMagnitude <= sqrRadius)
                    .ToList();

                foreach (var enemy in enemiesInRange)
                {
                    enemy.OnDamaged(_damage, _fontColor);
                }
            }

            // 2. 쾅! 터지는 폭발 파티클 띄우기
            BaseObject explosionEffect = null;
            if (poolManager != null && _explosionEffectPrefab != null)
            {
                // 유저님의 풀링 매니저 함수인 GetFromPool 적용
                explosionEffect = poolManager.GetFromPool<BaseObject>(_explosionEffectPrefab);

                if (explosionEffect != null)
                {
                    explosionEffect.transform.position = explosionCenter + new Vector3(0, 0.5f, 0);
                    explosionEffect.gameObject.SetActive(true);
                }
            }

            // 3. 파티클이 터질 시간(0.5초) 동안 대기
            yield return new WaitForSeconds(_effectDuration);

            // 4. 대기가 끝나면 파티클 끄고 풀에 반납
            if (poolManager != null && explosionEffect != null)
            {
                poolManager.ReturnPool(explosionEffect);
            }

            // 5. 총알도 임무를 완수했으므로 풀에 반납
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

        // BaseObject 상속 시 필수 구현부
        public override void ObjectUpdate() { }
    }
}