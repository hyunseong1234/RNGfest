using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Bullet;
using Dev.jeon.Effect;
using Dev.jeon.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class IceBoss : BaseBoss
    {
        [Header("Projectile Settings")]
        [SerializeField] private BossBullet _bulletPrefab;
        [SerializeField] private float _bulletSpeed = 15f;

        [Header("Scope Settings")]
        [SerializeField] private BaseObject _scopePrefab;

        //  [최적화] 메모리 할당 방지를 위한 캐싱 변수들
        private List<Tower> _tempValidTowers = new List<Tower>();
        private WaitForSeconds _skillWait;
        private float _lastWaitDuration = -1f;

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var towers = main.SpawnTowers;

            if (towers == null || towers.Count == 0) yield break;

            // 1. [할당 제거] 유효한 타워 리스트 채우기 (for 루프 사용)
            _tempValidTowers.Clear();
            for (int i = 0; i < towers.Count; i++)
            {
                if (!towers[i].IsSealed)
                {
                    _tempValidTowers.Add(towers[i]);
                }
            }

            if (_tempValidTowers.Count == 0) yield break;

            // 2. 타겟 랜덤 선택
            Tower target = _tempValidTowers[UnityEngine.Random.Range(0, _tempValidTowers.Count)];

            // 3. 조준 연출 시작
            BaseObject scopeObj = null;
            if (_scopePrefab != null)
            {
                scopeObj = pool.GetFromPool<BaseObject>(_scopePrefab);
                if (scopeObj != null)
                {
                    scopeObj.gameObject.SetActive(true);
                    scopeObj.transform.position = target.transform.position + Vector3.up * 0.1f;

                    // 스코프 스크립트 실행
                    if (scopeObj.TryGetComponent(out TargetScopeEffect scopeScript))
                    {
                        scopeScript.StartLockOn(_skillMotionDuration);
                    }
                }
            }

            // 4. [최적화] WaitForSeconds 캐싱 로직
            // Duration이 바뀌었을 때만 새로 생성하고, 평소에는 캐싱된 것을 재사용
            if (_lastWaitDuration != _skillMotionDuration)
            {
                _skillWait = new WaitForSeconds(_skillMotionDuration);
                _lastWaitDuration = _skillMotionDuration;
            }

            yield return _skillWait;

            // 5. 발사!
            if (target != null && target.gameObject.activeSelf)
            {
                var bullet = pool.GetFromPool<BossBullet>(_bulletPrefab);
                if (bullet != null)
                {
                    bullet.gameObject.SetActive(true);
                    bullet.transform.position = transform.position + Vector3.up * 2f;
                    bullet.InitBossSkill(target, _bulletSpeed);
                }
            }

            // 6. 스코프 반납
            if (scopeObj != null)
            {
                pool.ReturnPool(scopeObj);
            }
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _tempValidTowers.Clear(); // 리스트 정리
            _movedTileCount = 0;
            _lastWaypointIndex = 0;
        }
    }
}