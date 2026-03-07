using Dev.cheol.Manager;
using Dev.cheol.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class AdeleBullet : BaseBullet
    {
        private enum SwordState
        {
            Chasing,
            Overshooting,
            Returning
        }

        [Header("소환검 공격 설정")]
        [SerializeField] private float _attackSpeed = 40f;
        [SerializeField] private float _attackInterval = 0.2f;

        [Header("3D 입체 기동 및 불규칙성 설정")]
        [Tooltip("최소 ~ 최대 뚫고 지나가는 거리")]
        [SerializeField] private float _minOvershoot = 2.0f;
        [SerializeField] private float _maxOvershoot = 6.0f;

        [Tooltip("기수를 돌리기 전 대기 시간 (짧을수록 빠릿함)")]
        [SerializeField] private float _minTurnDelay = 0.05f;
        [SerializeField] private float _maxTurnDelay = 0.2f;

        [Tooltip("몬스터의 정중앙이 아닌, 무작위 3D 위치(어깨, 다리 등)를 찌르기 위한 오차 범위")]
        [SerializeField] private float _hitOffsetRadius = 1.0f;

        private Transform _owner;
        private Enemy _currentTarget;
        private Coroutine _behaviorCoroutine;
        private ObjectPoolingManger _pool;

        private SwordState _currentState = SwordState.Chasing;

        // --- 동적 기동을 위한 변수들 ---
        private Vector3 _targetAttackPos;
        private Vector3 _currentRandomOffset; // 이번 찌르기의 3D 오차 위치
        private float _currentOvershootLimit; // 이번에 뚫고 지나갈 무작위 거리
        private float _currentTurnDelay;      // 이번 턴의 무작위 딜레이
        private float _nextAttackTime = 0f;

        private void Awake()
        {
            _pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
        }

        public override void Init(Transform owner, float damage, float speed = 20f)
        {
            _owner = owner;
            _damage = damage;

            PrepareNextAttack(); // 첫 공격을 위한 무작위 값 세팅
            _currentState = SwordState.Chasing;

            if (_behaviorCoroutine != null) StopCoroutine(_behaviorCoroutine);
            _behaviorCoroutine = StartCoroutine(SwordRoutine());
        }

        private IEnumerator SwordRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();

            while (true)
            {
                if (_currentState != SwordState.Returning && (main.SpawnEnemys.Count == 0 || _owner == null))
                {
                    _currentState = SwordState.Returning;
                }

                switch (_currentState)
                {
                    case SwordState.Chasing:
                        if (_currentTarget == null || !_currentTarget.gameObject.activeSelf)
                        {
                            _currentTarget = FindNearestTarget(main.SpawnEnemys);
                            if (_currentTarget == null)
                            {
                                _currentState = SwordState.Returning;
                                break;
                            }
                            PrepareNextAttack(); // 타겟이 바뀌면 새로운 타격점 세팅
                        }

                        // 몬스터의 중심 + 3D 무작위 오차 공간을 향해 돌진 (상하좌우 입체적 찌르기)
                        _targetAttackPos = _currentTarget.transform.position + _currentRandomOffset;
                        transform.position = Vector3.MoveTowards(transform.position, _targetAttackPos, _attackSpeed * Time.deltaTime);
                        LookAtDirection(_targetAttackPos);

                        // 목표 지점(무작위 타격점)에 도달했을 때
                        if (Vector3.Distance(transform.position, _targetAttackPos) < 0.2f)
                        {
                            if (Time.time >= _nextAttackTime)
                            {
                                _currentTarget.OnDamaged(_damage, _fontColor);
                                _nextAttackTime = Time.time + _attackInterval;
                            }

                            // 뚫고 지나갈 무작위 거리와 딜레이를 새롭게 뽑음
                            _currentOvershootLimit = Random.Range(_minOvershoot, _maxOvershoot);
                            _currentTurnDelay = Random.Range(_minTurnDelay, _maxTurnDelay);

                            _currentState = SwordState.Overshooting;
                        }
                        break;

                    case SwordState.Overshooting:
                        // 뚫고 지나가기
                        transform.Translate(Vector3.forward * _attackSpeed * Time.deltaTime, Space.Self);

                        if (_currentTarget != null)
                        {
                            // 무작위로 설정된 거리(_currentOvershootLimit)만큼 멀어졌는가?
                            float distFromTarget = Vector3.Distance(transform.position, _currentTarget.transform.position);
                            if (distFromTarget >= _currentOvershootLimit)
                            {
                                yield return new WaitForSeconds(_currentTurnDelay);
                                PrepareNextAttack(); // 다시 돌진하기 전, 다음 찌를 3D 위치 재설정
                                _currentState = SwordState.Chasing;
                            }
                        }
                        else
                        {
                            PrepareNextAttack();
                            _currentState = SwordState.Chasing;
                        }
                        break;

                    case SwordState.Returning:
                        if (_owner != null)
                        {
                            transform.position = Vector3.MoveTowards(transform.position, _owner.position, (_attackSpeed * 0.5f) * Time.deltaTime);
                            LookAtDirection(_owner.position);

                            if (Vector3.Distance(transform.position, _owner.position) < 0.5f)
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

        // 다음 돌진을 위한 3D 타격 오차 범위를 생성하는 헬퍼 함수
        private void PrepareNextAttack()
        {
            // UnitSphere를 사용해 x, y, z 전방향으로 구체 형태의 무작위 좌표 생성
            // 몬스터의 중심만 때리지 않고, 머리 위, 다리 아래, 왼쪽 어깨 등으로 파고들게 만듦
            _currentRandomOffset = Random.insideUnitSphere * _hitOffsetRadius;
        }

        private Enemy FindNearestTarget(List<Enemy> enemies)
        {
            Enemy nearest = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.gameObject.activeSelf)
                {
                    float dist = Vector3.Distance(transform.position, enemy.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
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

        private void ReturnToPool()
        {
            if (_pool != null) _pool.ReturnPool(this);
        }

        private void OnDisable()
        {
            if (_behaviorCoroutine != null)
            {
                StopCoroutine(_behaviorCoroutine);
                _behaviorCoroutine = null;
            }
            _currentTarget = null;
            _owner = null;
        }

        public override void ObjectUpdate()
        {
        }
    }
}