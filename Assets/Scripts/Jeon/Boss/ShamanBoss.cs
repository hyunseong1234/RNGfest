using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Bullet;
using Dev.jeon.Model;
using Dev.jeon.Effect; // TargetScopeEffect 사용을 위해 추가
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class ShamanBoss : BaseBoss
    {
        [Header("Curse Settings")]
        [SerializeField] private BossBullet _bulletPrefab;
        [SerializeField] private float _bulletSpeed = 15f;

        [Header("Scope Settings")]
        [SerializeField] private BaseObject _scopePrefab;

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var towers = main.SpawnTowers;

            // 1. 타겟팅 대상 선정
            if (towers == null || towers.Count == 0) yield break;

            // 봉인되지 않은 타워 중 랜덤하게 섞기
            var availableTowers = towers.FindAll(t => !t.IsSealed).OrderBy(x => Random.value).ToList();
            if (availableTowers.Count == 0) yield break;

            int targetCount = Mathf.Min(2, availableTowers.Count);
            List<Tower> targetTowers = new List<Tower>();
            List<BaseObject> activeScopes = new List<BaseObject>();

            // 2. 조준 시작: 스코프 소환 및 배치
            for (int i = 0; i < targetCount; i++)
            {
                Tower target = availableTowers[i];
                targetTowers.Add(target);

                if (_scopePrefab != null)
                {
                    var scopeObj = pool.GetFromPool<BaseObject>(_scopePrefab);
                    if (scopeObj != null)
                    {
                        scopeObj.gameObject.SetActive(true);
                        scopeObj.transform.position = target.transform.position + Vector3.up * 0.1f;

                        if (scopeObj.TryGetComponent(out TargetScopeEffect scopeScript))
                        {
                            scopeScript.StartLockOn(_skillMotionDuration);
                        }
                        activeScopes.Add(scopeObj);
                    }
                }
            }

            // 3. 조준 시간 동안 대기 (1.5초)
            yield return new WaitForSeconds(_skillMotionDuration);

            // 4. 발사! (조준이 끝난 시점에도 타워가 살아있으면 발사)
            foreach (var target in targetTowers)
            {
                if (target != null && target.gameObject.activeSelf)
                {
                    var bullet = pool.GetFromPool<BossBullet>(_bulletPrefab.name);
                    if (bullet != null)
                    {
                        bullet.gameObject.SetActive(true);
                        bullet.transform.position = transform.position + Vector3.up * 2f;
                        bullet.InitBossSkill(target, _bulletSpeed);
                    }
                }
            }

            // 5. 스코프들 반납
            foreach (var scope in activeScopes)
            {
                if (scope != null) pool.ReturnPool(scope);
            }

            Debug.Log($"[주술사] {targetCount}개의 타워에 조준 후 저주 발사 완료!");
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _movedTileCount = 0;
            _lastWaypointIndex = 0;
        }
    }
}