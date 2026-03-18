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
        [SerializeField] private BaseObject _portalPrefab;
        [SerializeField] private BaseObject _impactEffectPrefab;

        [Header("Sound Settings")]
        [SerializeField] private AudioClip _portalSound; // 포탈 생성 소리
        [SerializeField] private AudioClip _impactSound; // 미니언 착지 소리

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var mapManager = ServiceLocator.Instance.GetService<MapManager>();
            var sm = ServiceLocator.Instance.GetService<SoundManager>();

            if (pool == null || mapManager == null || sm == null) yield break;

            Transform[] path = mapManager.FlagPoints;
            float portalDistanceBehind = 2.0f;
            Vector3 groundTargetBase = GetPositionBehindAlongPath(path, portalDistanceBehind);
            Vector3 portalSpawnPos = groundTargetBase + Vector3.up * 3.5f;

            // 1. 소환진 생성 및 사운드
            BaseObject portal = pool.GetFromPool<BaseObject>(_portalPrefab);
            if (portal != null)
            {
                if (_portalSound != null) sm.PlaySFX(_portalSound);
                portal.transform.position = portalSpawnPos;
                portal.transform.rotation = Quaternion.Euler(-45f, 180f, 0);
                portal.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(0.5f);

            // 2. 미니언 순차 소환
            for (int i = 0; i < _summonCount; i++)
            {
                var minion = pool.GetFromPool<SummonedMinion>(_minionPrefab);
                if (minion != null)
                {
                    float landDistance = portalDistanceBehind + (i * _spacing);
                    Vector3 landPos = GetPositionBehindAlongPath(path, landDistance);

                    // 미니언 소환 및 이동 루틴 시작
                    StartCoroutine(LandingRoutine(minion, portalSpawnPos, landPos, pool, sm));
                }
                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(1.5f);

            // 포탈 반납
            if (portal != null)
            {
                portal.gameObject.SetActive(false);
                pool.ReturnPool(portal);
            }
        }

        private IEnumerator LandingRoutine(SummonedMinion minion, Vector3 startPos, Vector3 landPos, ObjectPoolingManger pool, SoundManager sm)
        {
            float duration = 0.6f;
            float elapsed = 0f;
            Vector3 midPoint = (startPos + landPos) * 0.5f + Vector3.up * 1f;

            minion.transform.position = startPos;
            minion.gameObject.SetActive(true);

            // 포탈에서 지면으로 포물선 이동
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float accT = t * t;
                Vector3 m1 = Vector3.Lerp(startPos, midPoint, accT);
                Vector3 m2 = Vector3.Lerp(midPoint, landPos, accT);
                minion.transform.position = Vector3.Lerp(m1, m2, accT);

                if (m2 - m1 != Vector3.zero)
                    minion.transform.rotation = Quaternion.LookRotation(m2 - m1);

                yield return null;
            }

            // 착지 시 처리
            minion.transform.position = landPos;
            minion.transform.rotation = Quaternion.Euler(0, 180, 0);

            if (_impactSound != null) sm.PlaySFX(_impactSound);

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

            yield return new WaitForSeconds(0.1f);

            // [중요] 미니언 초기화 및 관리 리스트 등록
            // 1. 체력 및 웨이포인트 인덱스 설정 (보스가 가던 길부터 시작)
            minion.SetupMinion(this._stat.MaxHp.Value * 0.1f, this._waypointIndex);

            // 2. MainManager의 업데이트 리스트에 추가 (그래야 이동 로직이 실행됨)
            var main = ServiceLocator.Instance.GetService<MainManager>();
            if (main != null && !main.SpawnEnemys.Contains(minion))
            {
                main.SpawnEnemys.Add(minion);
            }

            // 3. 즉시 다음 목적지 탐색 시작
            minion.RefreshPath();
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
                    return Vector3.Lerp(currentPoint, targetPoint, distance / distToPrev);

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
            // 보스 고유 상태 초기화
            _movedTileCount = 0;
            _lastWaypointIndex = 0;
        }
    }
}