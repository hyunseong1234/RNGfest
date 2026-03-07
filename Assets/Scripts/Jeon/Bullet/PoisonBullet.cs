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
            // 타겟이 살아있는 동안 계속 추적
            while (_target != null && _target.gameObject.activeSelf)
            {
                // 타겟이 BaseObject이므로 .transform.position으로 접근
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    _target.transform.position,
                    _speed * Time.deltaTime
                );

                if (Vector3.Distance(transform.position, _target.transform.position) < 0.05f)
                {
                    HitTarget();
                    yield break;
                }

                yield return null;
            }

            // 날아가는 도중 몬스터가 다른 타워에 맞아 죽어서 풀로 돌아갔다면, 총알도 조용히 반납
            ReturnToPool();
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