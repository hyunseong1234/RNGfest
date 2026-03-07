using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Stats;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.cheol.Model
{
    public class AttackTower : Tower
    {
        private List<Enemy> _enemyList; // 적 리스트 캐싱용
        [SerializeField] private BaseBullet _bullet;

        public BaseBullet Bullet { get => _bullet; set => _bullet = value; }

        protected override void Awake()
        {
            base.Awake();
            // 서비스 로케이터에서 리스트 미리 가져오기 (매 프레임 호출 방지)
            var mainManager = ServiceLocator.Instance.GetService<MainManager>();
            if (mainManager != null)
            {
                _enemyList = mainManager.SpawnEnemys;
            }


        }

        public override void ObjectUpdate()
        {
            if (!IsTargetValid())
            {
                Target = FindNearestEnemy();
            }

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
            Debug.Log("불렛호출됨");
            BaseBullet bullet = ServiceLocator.Instance.GetService<ObjectPoolingManger>().GetFromPool<BaseBullet>(_bullet);
            bullet.transform.position = transform.position;
            bullet.Init(_target, _stat.Damage.Value, 5);

        }

        /// <summary>
        /// 현재 타겟이 공격 가능한 상태인지 확인
        /// </summary>
        private bool IsTargetValid()
        {
            // null 체크 및 오브젝트 활성화(풀링) 체크
            if (Target == null || !Target.gameObject.activeSelf)
                return false;

            // 적이 리스트에 여전히 존재하는지 체크 (죽어서 제거되었는지 확인)
            // 리스트가 참조 타입이라 MainManager에서 지우면 여기서도 안보임
            if (_enemyList == null || !_enemyList.Any(e => e.transform == Target))
                return false;

            // 사거리 체크 (유클리드 제곱 거리)
            if (IsOutOfRange())
                return false;

            return true;
        }

        private bool IsOutOfRange()
        {
            // Target이 null일 때 호출되지 않도록 IsTargetValid에서 순서 제어됨
            float sqrDistance = (Target.position - transform.position).sqrMagnitude;
            return sqrDistance > (_stat.Range.Value * _stat.Range.Value);
        }

        private Transform FindNearestEnemy()
        {
            if (_enemyList == null || _enemyList.Count == 0) return null;

            // LINQ로 활성화된 적 중 가장 가까운 놈 탐색
            // OrderBy는 성능을 먹으므로 적이 너무 많으면 루프문으로 교체 권장
            return _enemyList
                .Where(e => e != null && e.gameObject.activeSelf)
                .OrderBy(e => (e.transform.position - transform.position).sqrMagnitude)
                .Select(e => e.transform)
                .FirstOrDefault();
        }
    }




}
