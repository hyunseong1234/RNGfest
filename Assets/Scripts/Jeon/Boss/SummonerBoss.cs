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
        [SerializeField] private string _summonMobKey = "NormalEnemy";
        [SerializeField] private int _summonCount = 3;
        [SerializeField] private float _spacing = 0.5f;

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var mapManager = ServiceLocator.Instance.GetService<MapManager>();

            if (main == null || pool == null || mapManager == null) yield break;

            Transform[] path = mapManager.FlagPoints;

            for (int i = 0; i < _summonCount; i++)
            {
                var monster = pool.GetFromPool<Enemy>(_summonMobKey);
                if (monster != null)
                {
                    // [핵심 1] 풀링된 객체를 확실히 활성화
                    monster.gameObject.SetActive(true);

                    // 1. 위치 설정 (타일 경로 역추적)
                    float distanceBack = (i + 1) * _spacing;
                    monster.transform.position = GetPositionBehindAlongPath(path, distanceBack);

                    // 2. 능력치 설정 (체력 10%)
                    if (this._stat != null && monster._stat != null)
                    {
                        float minionHp = this._stat.MaxHp.Value * 0.1f;
                        monster._stat.MaxHp.BaseValue = minionHp;
                        monster._stat.CurrentHp = minionHp;

                        // [체크] 만약 몬스터 속도가 0이면 움직이지 않으니 기본값을 강제합니다.
                        if (monster._stat.Speed.BaseValue <= 0) monster._stat.Speed.BaseValue = 1.0f;
                    }

                    // 3. 이동 경로 강제 동기화
                    monster._waypointIndex = this._waypointIndex;
                    monster.Target = null;
                    monster.RefreshPath(); // 여기서 다음 목적지(Target)가 잡힘

                    // [핵심 2] 강제로 상태를 MOVE로 변경
                    // Enemy.cs의 ObjectUpdate를 기다리지 않고 즉시 걷기 코루틴을 실행시킵니다.
                    monster.ChangeState(EState.MOVE);

                    // 4. 매니저 리스트 등록 (MainManager가 인식하게 함)
                    if (!main.SpawnEnemys.Contains(monster))
                    {
                        main.SpawnEnemys.Add(monster);
                    }
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