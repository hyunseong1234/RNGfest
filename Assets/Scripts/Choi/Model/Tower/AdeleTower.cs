using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Bullet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Model
{
    // AttackTower와 똑같이 Tower를 상속받아 모든 기능을 물려받습니다.
    public class AdeleTower : Tower
    {
        [Header("아델 타워 전용 설정")]
        [SerializeField] private AdeleBullet _adeleBulletPrefab;
        [SerializeField] private int _maxSwordCount = 2;

        private List<AdeleBullet> _mySwords = new();

        protected override void Awake()
        {
            base.Awake();
        }

        public override void ObjectUpdate()
        {
            if (IsSealed) return;

            base.ObjectUpdate();

            _mySwords.RemoveAll(s => s == null || !s.gameObject.activeSelf);

            if (!IsTargetValid())
            {
                Target = FindNearestEnemy();
            }

            // 수정된 조건: 적이 있으면 일단 ATTACK(또는 조준) 상태 유지
            if (Target != null)
            {
                ChangeState(EState.ATTACK);
            }
            else
            {
                ChangeState(EState.IDLE);
            }
        }

        public override void ActiveAttack()
        {
            // 검이 이미 꽉 찼으면 더 소환 안 함
            if (_mySwords.Count >= _maxSwordCount) return;

            // 서비스 로케이터에서 풀 매니저 호출
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            AdeleBullet sword = pool.GetFromPool<AdeleBullet>(_adeleBulletPrefab);

            if (sword != null)
            {
                sword.transform.position = transform.position;

                // [수정] _stat.Speed.Value 대신 40f (또는 원하는 비행 속도)를 직접 넣습니다.
                sword.Init(this.transform, _stat.Damage.Value, 40f);

                _mySwords.Add(sword);
            }
        }

        // --- AttackTower에서 검증된 타겟팅 로직 ---
        private bool IsTargetValid()
        {
            if (Target == null || !Target.gameObject.activeSelf) return false;
            float sqrDistance = (Target.position - transform.position).sqrMagnitude;
            return sqrDistance <= (_stat.Range.Value * _stat.Range.Value);
        }

        private Transform FindNearestEnemy()
        {
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();
            if (mainManager == null || mainManager.SpawnEnemys.Count == 0) return null;

            return mainManager.SpawnEnemys
                .Where(e => e != null && e.gameObject.activeSelf)
                .OrderBy(e => (e.transform.position - transform.position).sqrMagnitude)
                .Select(e => e.transform)
                .FirstOrDefault();
        }
    }
}