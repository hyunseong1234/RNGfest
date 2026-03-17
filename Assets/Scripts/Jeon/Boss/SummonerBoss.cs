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
                    StartCoroutine(LandingRoutine(minion, portalSpawnPos, landPos, pool, sm));
                }
                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(1.5f);
            if (portal != null) { portal.gameObject.SetActive(false); pool.ReturnPool(portal); }
        }

        private IEnumerator LandingRoutine(SummonedMinion minion, Vector3 startPos, Vector3 landPos, ObjectPoolingManger pool, SoundManager sm)
        {
            float duration = 0.6f;
            float elapsed = 0f;
            Vector3 midPoint = (startPos + landPos) * 0.5f + Vector3.up * 1f;

            minion.transform.position = startPos;
            minion.gameObject.SetActive(true);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float accT = t * t;
                Vector3 m1 = Vector3.Lerp(startPos, midPoint, accT);
                Vector3 m2 = Vector3.Lerp(midPoint, landPos, accT);
                minion.transform.position = Vector3.Lerp(m1, m2, accT);
                if (m2 - m1 != Vector3.zero) minion.transform.rotation = Quaternion.LookRotation(m2 - m1);
                yield return null;
            }

            // 착지 시 사운드 및 이펙트
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

            yield return new WaitForSeconds(0.3f);
            minion.SetupMinion(this._stat.MaxHp.Value * 0.1f, this._waypointIndex);
        }

        private Vector3 GetPositionBehindAlongPath(Transform[] path, float distance)
        {
            Vector3 currentPoint = transform.position;
            int prevIdx = _waypointIndex - 1;
            while (distance > 0 && prevIdx >= 0)
            {
                Vector3 targetPoint = path[prevIdx].position;
                float distToPrev = Vector3.Distance(currentPoint, targetPoint);
                if (distance <= distToPrev) return Vector3.Lerp(currentPoint, targetPoint, distance / distToPrev);
                distance -= distToPrev; currentPoint = targetPoint; prevIdx--;
            }
            return currentPoint;
        }

        private IEnumerator ReturnEffectToPool(ObjectPoolingManger pool, BaseObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (effect != null) { effect.gameObject.SetActive(false); pool.ReturnPool(effect); }
        }

        public override void OnReturnToPool() { base.OnReturnToPool(); _movedTileCount = 0; _lastWaypointIndex = 0; }
    }
}