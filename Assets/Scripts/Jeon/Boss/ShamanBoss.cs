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
    public class ShamanBoss : BaseBoss
    {
        [Header("Curse Settings")]
        [SerializeField] private BossBullet _bulletPrefab;
        [SerializeField] private float _bulletSpeed = 15f;

        [Header("Scope Settings")]
        [SerializeField] private BaseObject _scopePrefab;

        [Header("Sound Settings")]
        [SerializeField] private AudioClip _scopeSound; // 조준 시 사운드
        [SerializeField] private AudioClip _castSound;  // 실제 발사 시 사운드

        private List<Tower> _tempAvailableTowers = new List<Tower>();
        private List<Tower> _targetTowers = new List<Tower>();
        private List<BaseObject> _activeScopes = new List<BaseObject>();

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var sound = ServiceLocator.Instance.GetService<SoundManager>();
            var towers = main.SpawnTowers;

            if (towers == null || towers.Count == 0) yield break;

            // 1. [할당 제거] 유효한 타워 수집 (봉인되지 않은 타워만)
            _tempAvailableTowers.Clear();
            for (int i = 0; i < towers.Count; i++)
            {
                if (!towers[i].IsSealed) _tempAvailableTowers.Add(towers[i]);
            }

            if (_tempAvailableTowers.Count == 0) yield break;

            // 2. [할당 제거] 타겟팅 대상 랜덤 선정 (최대 2개)
            _targetTowers.Clear();
            _activeScopes.Clear();
            int targetCount = Mathf.Min(2, _tempAvailableTowers.Count);

            for (int i = 0; i < targetCount; i++)
            {
                // Fisher-Yates 방식을 응용하여 중복 없이 타겟 선정
                int randomIndex = UnityEngine.Random.Range(i, _tempAvailableTowers.Count);
                Tower selected = _tempAvailableTowers[randomIndex];

                _tempAvailableTowers[randomIndex] = _tempAvailableTowers[i];
                _tempAvailableTowers[i] = selected;

                _targetTowers.Add(selected);

                // 3. 조준 연출 및 사운드 시작
                if (_scopePrefab != null)
                {
                    var scopeObj = pool.GetFromPool<BaseObject>(_scopePrefab);
                    if (scopeObj != null)
                    {
                        scopeObj.gameObject.SetActive(true);
                        scopeObj.transform.position = selected.transform.position + Vector3.up * 0.1f;

                        // 조준 사운드 재생
                        if (_scopeSound != null) sound.PlaySFX(_scopeSound);

                        if (scopeObj.TryGetComponent(out TargetScopeEffect scopeScript))
                        {
                            scopeScript.StartLockOn(_skillMotionDuration);
                        }
                        _activeScopes.Add(scopeObj);
                    }
                }
            }

            // 4. 조준 대기 (기 모으는 시간)
            yield return new WaitForSeconds(_skillMotionDuration);

            // 5. 발사 사운드 및 탄환 생성
            if (_targetTowers.Count > 0 && _castSound != null)
            {
                sound.PlaySFX(_castSound); // 기 모으기가 끝나고 쏘는 순간 사운드
            }

            for (int i = 0; i < _targetTowers.Count; i++)
            {
                var target = _targetTowers[i];
                if (target != null && target.gameObject.activeSelf)
                {
                    var bullet = pool.GetFromPool<BossBullet>(_bulletPrefab);
                    if (bullet != null)
                    {
                        bullet.gameObject.SetActive(true);
                        bullet.transform.position = transform.position + Vector3.up * 2f;

                        // 발사음은 BossBullet 내부의 base.Init()에서도 재생됩니다.
                        bullet.InitBossSkill(target, _bulletSpeed);
                    }
                }
            }

            // 6. 스코프 반납
            for (int i = 0; i < _activeScopes.Count; i++)
            {
                if (_activeScopes[i] != null) pool.ReturnPool(_activeScopes[i]);
            }
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            // 리스트들 정리하여 메모리 누수 방지
            _tempAvailableTowers.Clear();
            _targetTowers.Clear();
            _activeScopes.Clear();
        }
    }
}