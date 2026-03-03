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
        private float _speed = 20f;
        [SerializeField] private int _damage = 10;

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
            // 타겟이 BaseUnit을 상속받은 Enemy인지 확인 (is 캐스팅)
            if (enemy != null)
            {
                enemy.OnDamaged(_damage); // 1. 총알 자체 데미지 (깡뎀)

                // 2. 적이 이미 독에 걸려있는지 확인!
                var existingPoison = enemy.GetBuff<PoisonBuff>();

                if (existingPoison != null)
                {
                    // [이미 걸려있음] 버프 시간만 다시 5초로 리셋!
                    existingPoison.Refresh(5.0f);
                    Debug.Log("<color=yellow>[독 갱신]</color> 지속 시간이 다시 5초로 늘어났습니다!");
                }
                else
                {
                    // [안 걸려있음] 새 독 버프를 만들어서 주입
                    var newPoison = new PoisonBuff(5); // 틱당 5뎀
                    newPoison.Init(enemy, 5.0f);       // 5초 지속
                    enemy.AddBuff(newPoison);
                    Debug.Log("<color=orange>[독 감염]</color> 적이 독에 감염되었습니다! (5초 지속)");
                }

                // 임무를 마친 총알은 풀로 복귀
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