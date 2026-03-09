using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class SummonerBoss : BaseBoss
    {
        [Header("Summon Settings")]
        [SerializeField] private SummonedMinion _minionPrefab;
        [SerializeField] private int _summonCount = 3;
        [SerializeField] private float _spacing = 0.5f;

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var mapManager = ServiceLocator.Instance.GetService<MapManager>();
            if (pool == null || mapManager == null) yield break;

            Transform[] path = mapManager.FlagPoints;

            for (int i = 0; i < _summonCount; i++)
            {
                var minion = pool.GetFromPool<SummonedMinion>(_minionPrefab);

                if (minion != null)
                {
                    // [가장 중요] 오브젝트를 먼저 활성화해야 ChangeState의 코루틴이 작동합니다.
                    minion.gameObject.SetActive(true);

                    // 1. 위치 설정
                    float distanceBack = (i + 1) * _spacing;
                    minion.transform.position = GetPositionBehindAlongPath(path, distanceBack);

                    // 2. 세팅 호출 (이제 활성화 상태이므로 내부의 ChangeState가 정상 작동함)
                    float hp = this._stat.MaxHp.Value * 0.1f;
                    minion.SetupMinion(hp, this._waypointIndex);
                }
            }
            yield return null;
        }

        private Vector3 GetPositionBehindAlongPath(Transform[] path, float distance)
        {
            Vector3 currentPoint = transform.position;
            int prevIdx = _waypointIndex - 1;

            while (distance > 0 && prevIdx >= 0)
            {
                Vector3 targetPoint = path[prevIdx].position;
                float distToPrev = Vector3.Distance(currentPoint, targetPoint);

                if (distance <= distToPrev)
                {
                    return Vector3.Lerp(currentPoint, targetPoint, distance / distToPrev);
                }

                distance -= distToPrev;
                currentPoint = targetPoint;
                prevIdx--;
            }
            return currentPoint;
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _movedTileCount = 0;
            _lastWaypointIndex = 0;
        }
    }
}