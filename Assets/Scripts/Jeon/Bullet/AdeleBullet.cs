using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class AdeleBullet : BaseBullet
    {
        private enum SwordState { Chasing, Overshooting, Returning }

        [Header("소환검 공격 설정")]
        [SerializeField] private float _attackSpeed = 40f;
        [SerializeField] private float _attackInterval = 0.2f;

        [Header("3D 입체 기동 설정")]
        [SerializeField] private float _minOvershoot = 2.0f;
        [SerializeField] private float _maxOvershoot = 6.0f;
        [SerializeField] private float _minTurnDelay = 0.05f;
        [SerializeField] private float _maxTurnDelay = 0.2f;

        private SwordState _currentState = SwordState.Chasing;
        private Transform _owner;
        private Enemy _currentTarget;
        private Coroutine _behaviorCoroutine;

        private float _nextAttackTime = 0f;
        private float _currentOvershootLimit;
        private float _currentTurnDelay;

        // BaseBullet의 추상 메서드 구현
        public override void Init(Transform targetOrOwner, float damage, float speed = 40f)
        {
            _owner = targetOrOwner;
            _damage = damage;
            _attackSpeed = speed;
            _currentState = SwordState.Chasing;

            if (_behaviorCoroutine != null) StopCoroutine(_behaviorCoroutine);
            _behaviorCoroutine = StartCoroutine(SwordRoutine());
        }

        // BaseBullet의 추상 메서드 구현 (검은 코루틴으로 돌리므로 비워둠)
        public override void ObjectUpdate() { }

        private IEnumerator SwordRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            float searchTimer = 0f;

            // [최적화] 루프 시작 전 주인의 타워 스크립트를 미리 찾아둡니다 (캐싱)
            Tower ownerTower = _owner != null ? _owner.GetComponent<Tower>() : null;

            while (true)
            {
                // --- 실시간 상태 체크 ---
                bool shouldReturn = false;

                // 1. 주인이 파괴되었는가?
                if (_owner == null)
                {
                    shouldReturn = true;
                }
                // 2. 주인이 쉬고(IDLE) 있는가? (적이 사거리 밖으로 나갔을 때 등)
                else if (ownerTower != null && ownerTower.CurrentState == EState.IDLE)
                {
                    shouldReturn = true;
                }
                // 3. 맵에 적이 아예 없는가?
                else if (main.SpawnEnemys.Count == 0)
                {
                    shouldReturn = true;
                }

                // 체크 결과 돌아가야 한다면 상태 변경
                if (_currentState != SwordState.Returning && shouldReturn)
                {
                    _currentState = SwordState.Returning;
                }
                // -------------------------

                switch (_currentState)
                {
                    case SwordState.Chasing:
                        // ... 기존 추적 로직 ...
                        if (_currentTarget == null || !_currentTarget.gameObject.activeSelf)
                        {
                            searchTimer += Time.deltaTime;
                            if (searchTimer >= 0.1f)
                            {
                                _currentTarget = FindNearestTarget(main.SpawnEnemys);
                                searchTimer = 0f;
                            }

                            if (_currentTarget == null) break;
                        }

                        Vector3 targetPos = _currentTarget.transform.position;
                        transform.position = Vector3.MoveTowards(transform.position, targetPos, _attackSpeed * Time.deltaTime);
                        LookAtDirection(targetPos);

                        // sqrMagnitude 사용: $$d^2 = (x_2-x_1)^2 + (y_2-y_1)^2 + (z_2-z_1)^2$$
                        if ((transform.position - targetPos).sqrMagnitude < 0.04f)
                        {
                            if (Time.time >= _nextAttackTime)
                            {
                                _currentTarget.OnDamaged(_damage, _fontColor);
                                _nextAttackTime = Time.time + _attackInterval;
                            }

                            _currentOvershootLimit = Random.Range(_minOvershoot, _maxOvershoot);
                            _currentTurnDelay = Random.Range(_minTurnDelay, _maxTurnDelay);
                            _currentState = SwordState.Overshooting;
                        }
                        break;

                    case SwordState.Overshooting:
                        // ... 기존 관통 로직 ...
                        transform.Translate(Vector3.forward * _attackSpeed * Time.deltaTime, Space.Self);

                        if (_currentTarget != null)
                        {
                            float sqrDist = (transform.position - _currentTarget.transform.position).sqrMagnitude;
                            if (sqrDist >= (_currentOvershootLimit * _currentOvershootLimit))
                            {
                                yield return new WaitForSeconds(_currentTurnDelay);
                                _currentState = SwordState.Chasing;
                            }
                        }
                        else { _currentState = SwordState.Chasing; }
                        break;

                    case SwordState.Returning:
                        // 복귀 로직
                        if (_owner != null)
                        {
                            transform.position = Vector3.MoveTowards(transform.position, _owner.position, _attackSpeed * Time.deltaTime);
                            LookAtDirection(_owner.position);

                            if ((transform.position - _owner.position).sqrMagnitude < 0.25f)
                            {
                                ReturnToPool();
                                yield break;
                            }
                        }
                        else
                        {
                            ReturnToPool();
                            yield break;
                        }
                        break;
                }
                yield return null;
            }
        }
        private Enemy FindNearestTarget(List<Enemy> enemies)
        {
            Enemy nearest = null;
            float minSqrDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.gameObject.activeSelf)
                {
                    float sqrDist = (transform.position - enemy.transform.position).sqrMagnitude;
                    if (sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        nearest = enemy;
                    }
                }
            }
            return nearest;
        }

        private void LookAtDirection(Vector3 targetPos)
        {
            Vector3 dir = targetPos - transform.position;
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        protected override void ReturnToPool()
        {
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            if (pool != null) pool.ReturnPool(this);
        }

        private void OnDisable()
        {
            if (_behaviorCoroutine != null) StopCoroutine(_behaviorCoroutine);
            _currentTarget = null;
            _owner = null;
        }
    }
}