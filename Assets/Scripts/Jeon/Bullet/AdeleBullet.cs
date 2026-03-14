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

        // 1. 초기화 (부모의 Init을 활용하여 발사음 자동 재생)
        public override void Init(Transform targetOrOwner, float damage, float speed = 40f)
        {
            _owner = targetOrOwner;
            _damage = damage;
            _attackSpeed = speed;
            _currentState = SwordState.Chasing;

            // 부모의 Init을 호출하여 발사음(_fireSound) 재생
            // 단, 부모의 StartMove(선형 이동)는 아델검에게 맞지 않으므로 아래에서 직접 제어
            base.Init(targetOrOwner, damage, speed);
        }

        // 2. 이동 방식 정의 (부모의 선형 이동 대신 아델 전용 루틴 실행)
        protected override void StartMove()
        {
            if (_behaviorCoroutine != null) StopCoroutine(_behaviorCoroutine);
            _behaviorCoroutine = StartCoroutine(SwordRoutine());
        }

        // 3. 필수 구현 (컴파일 에러 해결 구간)
        // 아델검은 충돌 순간이 루틴 내부에 있으므로 여기서는 공통 데미지 로직만 작성하거나 비워둠
        protected override void ApplyHitLogic(Vector3 hitPoint)
        {
            if (_currentTarget != null)
            {
                _currentTarget.OnDamaged(_damage, _fontColor);
                // 타격 사운드와 이펙트는 루틴 내에서 직접 호출하므로 여기선 생략 가능
            }
        }

        private IEnumerator SwordRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            float searchTimer = 0f;
            Tower ownerTower = _owner != null ? _owner.GetComponent<Tower>() : null;

            while (true)
            {
                // 실시간 상태 체크 (주인 파괴, 적 전멸 등)
                bool shouldReturn = (_owner == null) ||
                                    (ownerTower != null && ownerTower.CurrentState == EState.IDLE) ||
                                    (main.SpawnEnemys.Count == 0);

                if (_currentState != SwordState.Returning && shouldReturn)
                {
                    _currentState = SwordState.Returning;
                }

                switch (_currentState)
                {
                    case SwordState.Chasing:
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

                        // 적중 체크
                        if ((transform.position - targetPos).sqrMagnitude < 0.04f)
                        {
                            if (Time.time >= _nextAttackTime)
                            {
                                // 데미지 입히기
                                ApplyHitLogic(transform.position);

                                // 부모의 공통 함수를 사용하여 타격음과 이펙트 재생
                                PlaySound(_hitSound);
                                SpawnHitEffect(transform.position);

                                _nextAttackTime = Time.time + _attackInterval;
                            }

                            _currentOvershootLimit = Random.Range(_minOvershoot, _maxOvershoot);
                            _currentTurnDelay = Random.Range(_minTurnDelay, _maxTurnDelay);
                            _currentState = SwordState.Overshooting;
                        }
                        break;

                    case SwordState.Overshooting:
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
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
        }

        // 부모의 ReturnToPool을 호출하여 코루틴 중지 및 안전한 반납 처리
        protected override void ReturnToPool()
        {
            if (_behaviorCoroutine != null) StopCoroutine(_behaviorCoroutine);
            base.ReturnToPool();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _currentTarget = null;
            _owner = null;
        }
    }
}