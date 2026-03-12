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

        [Header("Effect Settings")]
        [SerializeField] private BaseObject _portalPrefab;      // Portal blue 프리팹
        [SerializeField] private BaseObject _impactEffectPrefab; // 바닥 착지/먼지 프리팹

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var mapManager = ServiceLocator.Instance.GetService<MapManager>();
            if (pool == null || mapManager == null) yield break;

            Transform[] path = mapManager.FlagPoints;

            // 1. 소환진 위치 계산 (보스 위치 2타일 뒤 지점의 높이 +5)
            float portalDistanceBehind = 2.0f;
            Vector3 groundTargetBase = GetPositionBehindAlongPath(path, portalDistanceBehind);
            Vector3 portalSpawnPos = groundTargetBase + Vector3.up * 3.5f;

            // 2. 소환진(Portal blue) 생성
            BaseObject portal = pool.GetFromPool<BaseObject>(_portalPrefab);
            if (portal != null)
            {
                portal.transform.position = portalSpawnPos;
                portal.transform.rotation = Quaternion.Euler(-45f, 180f, 0);
                portal.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(0.5f);

            // 3. 미니언 순차 소환 (곡선 낙하 연출 포함)
            for (int i = 0; i < _summonCount; i++)
            {
                var minion = pool.GetFromPool<SummonedMinion>(_minionPrefab);

                if (minion != null)
                {
                    float landDistance = portalDistanceBehind + (i * _spacing);
                    Vector3 landPos = GetPositionBehindAlongPath(path, landDistance);

                    StartCoroutine(LandingRoutine(minion, portalSpawnPos, landPos, pool));
                }

                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(1.5f);
            if (portal != null)
            {
                portal.gameObject.SetActive(false);
                pool.ReturnPool(portal);
            }
        }

        private IEnumerator LandingRoutine(SummonedMinion minion, Vector3 startPos, Vector3 landPos, ObjectPoolingManger pool)
        {
            float duration = 0.6f;
            float elapsed = 0f;

            Vector3 midPoint = (startPos + landPos) * 0.5f + Vector3.up * 1f;

            minion.transform.position = startPos;
            minion.gameObject.SetActive(true);

            // Step 1: 곡선 낙하 (베지어 곡선)
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float acceleratedT = t * t;

                Vector3 m1 = Vector3.Lerp(startPos, midPoint, acceleratedT);
                Vector3 m2 = Vector3.Lerp(midPoint, landPos, acceleratedT);
                minion.transform.position = Vector3.Lerp(m1, m2, acceleratedT);

                if (m2 - m1 != Vector3.zero)
                    minion.transform.rotation = Quaternion.LookRotation(m2 - m1);

                yield return null;
            }

            // Step 2: 착지
            minion.transform.position = landPos;
            minion.transform.rotation = Quaternion.Euler(0, 180, 0);

            // Step 3: 착지 이펙트 생성
            if (_impactEffectPrefab != null)
            {
                var impact = pool.GetFromPool<BaseObject>(_impactEffectPrefab);
                if (impact != null)
                {
                    impact.transform.position = landPos;
                    impact.gameObject.SetActive(true);
                    pool.StartCoroutine(ReturnEffectToPool(pool, impact, 1.5f));
                }
            }

            // Step 4: 착지 후 0.3초 대기
            yield return new WaitForSeconds(0.3f);

            // Step 5: 미니언 세팅
            float hp = this._stat.MaxHp.Value * 0.1f;
            minion.SetupMinion(hp, this._waypointIndex);
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

        private IEnumerator ReturnEffectToPool(ObjectPoolingManger pool, BaseObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (effect != null)
            {
                effect.gameObject.SetActive(false);
                pool.ReturnPool(effect);
            }
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _movedTileCount = 0;
            _lastWaypointIndex = 0;
        }
    }
}