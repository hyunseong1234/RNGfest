using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Stats; // 순수 C# 버프(PoisonBuff)가 있는 네임스페이스
using Dev.jeon.Model;
using System.Collections;
using UnityEngine;

namespace Dev.jeon.Bullet
{
    // 유저님의 완벽한 뼈대인 BaseBullet을 상속받습니다!
    public class PoisonBullet : BaseBullet
    {

        [SerializeField] private int _poisonDamage = 5;  // 틱당 들어가는 독뎀
        private Coroutine _moveCoroutine;

        // damage 와 poisonDamage의 분리
        public override void Init(Transform target, float damage, float speed = 20f)
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
            // 1. 타겟의 마지막 위치를 기억할 변수 초기화
            Vector3 lastTargetPos = _target.position;

            // 2. 루프 조건: 타겟 생존 여부와 상관없이 '도착할 때까지' 계속 실행
            while (true)
            {
                // 타겟이 살아있다면 실시간으로 목표 좌표 갱신 (추적)
                if (_target != null && _target.gameObject.activeSelf)
                {
                    lastTargetPos = _target.position;
                }

                // 3. '마지막으로 확인된 위치'를 향해 이동
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    lastTargetPos,
                    _speed * Time.deltaTime
                );

                // 4. 도착 체크 (타겟 오브젝트가 아니라 '저장된 좌표'와 비교)
                if (Vector3.Distance(transform.position, lastTargetPos) < 0.05f)
                {
                    HitTarget();
                    yield break; // 코루틴 종료
                }

                yield return null;
            }
        }
        private void HitTarget()
        {
            var enemy = _target.GetComponent<Enemy>();

            if (enemy != null)
            {
                // 1. 총알 자체 데미지 (깡뎀 적용)
                enemy.OnDamaged(_damage, _fontColor);

                // 2. 적이 이미 독에 걸려있는지 확인
                var existingPoison = enemy.GetBuff<PoisonBuff>();

                if (existingPoison != null)
                {
                    // [이미 걸려있음] 새로운 독뎀이 기존 독뎀보다 쎈지 비교!
                    if (_poisonDamage > existingPoison.Damage)
                    {
                        existingPoison.UpgradePoison(_poisonDamage); // 더 강한 독으로 갱신
                        // Debug.Log($"<color=yellow>[독 강화]</color> {_poisonDamage} 맹독으로 갱신!");
                    }
                }
                else
                {
                    // [안 걸려있음] 새 독 버프 주입
                    var newPoison = new PoisonBuff(_poisonDamage); // 독뎀 전달

                    // 무한 지속(PositiveInfinity)으로 설정 (BaseUnit이 죽을 때 알아서 꺼줌)
                    newPoison.Init(enemy, float.PositiveInfinity);

                    enemy.AddBuff(newPoison);
                    // Debug.Log($"<color=orange>[독 감염]</color> 적이 맹독({_poisonDamage})에 감염됨!");
                }

                ReturnToPool();
            }
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

        // BaseObject 상속 시 필수 구현부 (코루틴으로 이동하므로 비워둠)
        public override void ObjectUpdate() { }


    }
}