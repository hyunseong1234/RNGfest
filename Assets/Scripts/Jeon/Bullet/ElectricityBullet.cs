using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class ElectricityBullet : BaseBullet
    {
        [Header("전기 속성 설정")]
        [SerializeField] private float _speed = 20f;
        [SerializeField] private int _damage = 10;
        [SerializeField] private float _bounceRadius = 5f;// 번개가 다음 적에게 튕길 수 있는 최대 사거리

        private int _maxTargets = 3; // 팅기는 횟수

        private float[] _damageMultipliers = { 1.0f, 0.7f, 0.4f };

        private Coroutine _moveCoroutine;

        public override void Init(Transform target, int damage, float speed = 20)
        {
            _target = target; ;
            _damage = damage;
            _speed = speed;

            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveToTarget());
        }

        private IEnumerator MoveToTarget()
        {
            while(_target != null && _target.gameObject.activeSelf)
            {
                transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, _speed * Time.deltaTime);

                if (Vector3.Distance(transform.position, _target.transform.position) < 0.05f)
                {
                    HitTarget();
                    yield break;
                }
                yield return null;
            }


            ReturnToPool();
        }

        private void HitTarget()
        {
            var primaryEnemy = _target.GetComponent<Enemy>();
            if (primaryEnemy == null)
            {
                ReturnToPool();
                return;
            }

            // 1. 이미 번개를 맞은 적을 추적하는 리스트 (왔다갔다 중복 타격 방지)
            List<Enemy> hitEnemies = new List<Enemy>();
            Enemy currentTarget = primaryEnemy;

            // 2. 최대 3마리 타격 루프
            for (int i = 0; i < _maxTargets; i++)
            {
                // 타겟이 죽었거나 비활성화면 연쇄 종료
                if (currentTarget == null || !currentTarget.gameObject.activeSelf) break;

                // 3. 순서에 맞는 데미지 계산 (100% -> 70% -> 40%)
                int finalDamage = Mathf.RoundToInt(_damage * _damageMultipliers[i]);
                currentTarget.OnDamaged(finalDamage);
                hitEnemies.Add(currentTarget);

                Debug.Log($"<color=cyan>[체인 라이트닝]</color> {i + 1}번째 타겟({currentTarget.name}) 적중! 데미지: {finalDamage}");

                // 4. 다음 타겟 찾기 (마지막 타격이 아닐 때만)
                if (i < _maxTargets - 1)
                {
                    Enemy nextTarget = FindNextTarget(currentTarget, hitEnemies);

                    // 다음 타겟이 주변에 없으면 그대로 연쇄 종료
                    if (nextTarget == null) break;

                    // 다음 루프를 위해 타겟 교체
                    currentTarget = nextTarget;
                }
            }

            // 연쇄 공격이 전부 끝난 후 총알 반납
            ReturnToPool();
        }

        private Enemy FindNextTarget(Enemy currentEnemy, List<Enemy> alreadyHit)
        {
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();
            if (mainManager == null) return null;

            return mainManager.SpawnEnemys
                .Where(e => e != null && e.gameObject.activeSelf) // 살아있는 적만
                .Where(e => !alreadyHit.Contains(e)) //  핵심: 이미 번개 맞은 놈은 제외!
                .Where(e => (e.transform.position - currentEnemy.transform.position).sqrMagnitude <= (_bounceRadius * _bounceRadius)) // 튕기는 사거리 안쪽
                .OrderBy(e => (e.transform.position - currentEnemy.transform.position).sqrMagnitude) // 가장 가까운 순서대로 정렬
                .FirstOrDefault(); // 첫 번째 놈(가장 가까운 놈) 픽!
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
