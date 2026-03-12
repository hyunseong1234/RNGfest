using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Stats;
using Dev.jeon.Model;
using System.Collections;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    public class PoisonBullet : BaseBullet
    {
        [Header("독성 설정")]
        [SerializeField] private int _poisonDamage = 5;      // 틱당 데미지
        [SerializeField] private float _poisonDuration = 5.0f; // 독 유지 시간

        [Header("연속 연출 설정")]
        [SerializeField] private BaseObject _poisonBuffVFXPrefab; // 독에 걸린 동안 유지될 이펙트

        private Coroutine _moveCoroutine;

        public override void Init(Transform target, float damage, float speed = 20f)
        {
            _target = target;
            _damage = damage;
            _speed = speed;

            // 기존에 실행 중인 이동 코루틴이 있다면 중지
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveToTarget());
        }

        private IEnumerator MoveToTarget()
        {
            // 타겟의 위치 초기화
            Vector3 lastTargetPos = _target.position;

            while (true)
            {
                // 타겟이 살아있다면 위치 실시간 갱신
                if (_target != null && _target.gameObject.activeSelf)
                {
                    lastTargetPos = _target.position;
                }

                // 목표 지점으로 이동
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    lastTargetPos,
                    _speed * Time.deltaTime
                );

                // 도착 확인
                if (Vector3.Distance(transform.position, lastTargetPos) < 0.05f)
                {
                    HitTarget();
                    yield break;
                }

                yield return null;
            }
        }

        private void HitTarget()
        {
            // 방어 코드: 타겟이 사라졌는지 확인
            if (_target == null)
            {
                ReturnToPool();
                return;
            }

            var enemy = _target.GetComponent<Enemy>();

            if (enemy != null)
            {
                // 1. 총알 자체의 즉발 데미지 적용
                enemy.OnDamaged(_damage, _fontColor);

                // 2. 맞는 순간 터지는 피격 이펙트 생성 (부모 클래스 함수)
                SpawnHitEffect(transform.position);

                // 3. 독 버프 로직 처리
                var existingPoison = enemy.GetBuff<PoisonBuff>();

                if (existingPoison != null)
                {
                    // 이미 중독 상태라면 데미지 비교 후 갱신
                    if (_poisonDamage > existingPoison.Damage)
                    {
                        existingPoison.UpgradePoison(_poisonDamage);
                    }

                    // 지속 시간만 초기화
                    existingPoison.Refresh(_poisonDuration);
                }
                else
                {
                    // 새로 중독 시 독 버프 생성
                    var newPoison = new PoisonBuff(_poisonDamage);

                    // 중요: 총알이 들고 있는 지속 연출 프리팹을 버프에 전달
                    newPoison.Init(enemy, _poisonDuration, _poisonBuffVFXPrefab);

                    // 적에게 버프 추가
                    enemy.AddBuff(newPoison);
                }
            }

            ReturnToPool();
        }

        protected override void ReturnToPool()
        {
            // 풀링 매니저를 통해 자신을 반납
            ServiceLocator.Instance.GetService<ObjectPoolingManger>().ReturnPool(this);
        }

        private void OnDisable()
        {
            // 오브젝트가 비활성화될 때 코루틴 및 변수 정리
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