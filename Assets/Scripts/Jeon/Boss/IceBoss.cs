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
        [Header("스킬 설정")]
        [SerializeField] private BossBullet _bulletPrefab;
        [SerializeField] private float _bulletSpeed = 15f;
        [SerializeField] private BaseObject _scopePrefab;

        [Header("사운드 설정")]
        [SerializeField] private AudioClip _scopeSound; // 조준 소리

        private List<Tower> _tempValidTowers = new List<Tower>();

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var sm = ServiceLocator.Instance.GetService<SoundManager>();

            // 유효한 타겟 수집
            _tempValidTowers.Clear();
            foreach (var t in main.SpawnTowers) if (!t.IsSealed) _tempValidTowers.Add(t);
            if (_tempValidTowers.Count == 0) yield break;

            Tower target = _tempValidTowers[Random.Range(0, _tempValidTowers.Count)];

            // 1. 조준 연출 및 사운드
            if (_scopePrefab != null)
            {
                var scopeObj = pool.GetFromPool<BaseObject>(_scopePrefab);
                if (scopeObj != null)
                {
                    scopeObj.transform.position = target.transform.position + Vector3.up * 0.1f;
                    scopeObj.gameObject.SetActive(true);

                    // 조준 사운드 재생
                    if (_scopeSound != null) sm.PlaySFX(_scopeSound);

                    if (scopeObj.TryGetComponent(out TargetScopeEffect scopeScript))
                        scopeScript.StartLockOn(_skillMotionDuration);

                    yield return new WaitForSeconds(_skillMotionDuration);
                    pool.ReturnPool(scopeObj);
                }
            }

            // 2. 발사
            if (target != null && target.gameObject.activeSelf)
            {
                var bullet = pool.GetFromPool<BossBullet>(_bulletPrefab);
                if (bullet != null)
                {
                    bullet.transform.position = transform.position + Vector3.up * 2f;
                    bullet.gameObject.SetActive(true);
                    bullet.InitBossSkill(target, _bulletSpeed);
                }
            }
        }
    }
}