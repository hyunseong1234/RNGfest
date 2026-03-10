using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using Dev.jeon.Bullet;
using Dev.jeon.Effect; // TargetScopeEffect 사용을 위해 추가
using System.Collections;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class IceBoss : BaseBoss
    {
        [Header("Projectile Settings")]
        [SerializeField] private SkillBullet _bulletPrefab;
        [SerializeField] private float _bulletSpeed = 15f;

        [Header("Scope Settings")]
        [SerializeField] private BaseObject _scopePrefab;

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            if (main.SpawnTowers == null || main.SpawnTowers.Count == 0) yield break;

            // 1. 타겟 타워 랜덤 선택
            var availableTowers = main.SpawnTowers.FindAll(t => !t.IsSealed);
            if (availableTowers.Count == 0) yield break;

            int rand = Random.Range(0, availableTowers.Count);
            Tower target = availableTowers[rand];

            // 2.  조준 시작: 스코프 소환
            BaseObject scopeObj = null;
            if (_scopePrefab != null)
            {
                scopeObj = pool.GetFromPool<BaseObject>(_scopePrefab);
                if (scopeObj != null && scopeObj.TryGetComponent(out TargetScopeEffect scopeScript))
                {
                    // 타워 위치에 스코프 배치 (살짝 위로 0.1f)
                    scopeObj.transform.position = target.transform.position + Vector3.up * 0.1f;
                    // 1.5초 동안 조준 연출 실행 (부모의 _skillMotionDuration 사용)
                    scopeScript.StartLockOn(_skillMotionDuration);
                }
            }

            // 3.  조준 시간 동안 대기 (1.5초)
            yield return new WaitForSeconds(_skillMotionDuration);

            // 4.  발사! (조준이 끝난 시점에 타워가 아직 있으면 발사)
            if (target != null && target.gameObject.activeSelf)
            {
                var bullet = pool.GetFromPool<SkillBullet>(_bulletPrefab);
                if (bullet != null)
                {
                    bullet.transform.position = transform.position + Vector3.up * 2f;
                    bullet.InitSkill(target, _bulletSpeed, SkillBullet.ESkillType.ICE);
                }
            }

            // 5. 스코프 반납
            if (scopeObj != null)
            {
                pool.ReturnPool(scopeObj);
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