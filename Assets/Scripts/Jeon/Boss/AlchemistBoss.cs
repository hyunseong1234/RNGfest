using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.jeon.Model;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Boss
{
    public class AlchemistBoss : BaseBoss
    {
        [Header("Alchemist Settings")]
        [SerializeField] private float _healPercent = 0.05f;
        [SerializeField] private float _healDelay = 1.0f;

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            if (main == null || pool == null) yield break;

            // 1. 강제 합성 로직 (기존과 동일)
            HandleForceMerge(main);

            // 2. 기획 조건: 1초 대기
            yield return new WaitForSeconds(_healDelay);

            // 3. 체력 회복 및 폰트 출력
            if (_stat != null)
            {
                float healAmount = _stat.MaxHp.Value * _healPercent;
                _stat.CurrentHp += healAmount;
                if (_stat.CurrentHp > _stat.MaxHp.Value) _stat.CurrentHp = _stat.MaxHp.Value;

                // [핵심] 회복 폰트 띄우기
                ShowHealFont(pool, main, healAmount);

                Debug.Log($"[연금술사 보스] {healAmount} 회복 완료!");
            }
        }

        private void ShowHealFont(ObjectPoolingManger pool, MainManager main, float amount)
        {
            // Enemy.cs에 있는 데미지 폰트 로직을 그대로 활용합니다.
            var healFont = pool.GetFromPool<DamageFont>("DamageFont");
            if (healFont != null)
            {
                // 데미지 폰트 사이즈 크기 증가 3배로 증가
                healFont.transform.localScale = Vector3.one * 3.0f;
                healFont.SetDamage(amount, transform, FontColor.Yellow);

                // UI 리스트에 추가하여 화면에 표시
                main.SpawnUI.Add(healFont);
            }
        }

        private void HandleForceMerge(MainManager main)
        {
            var activeTowers = main.SpawnTowers.Where(t => t != null && t.gameObject.activeSelf).ToList();
            var mergeableGroups = activeTowers.GroupBy(t => t.Lank).Where(g => g.Count() >= 2).ToList();

            if (mergeableGroups.Count > 0)
            {
                var targetGroup = mergeableGroups[Random.Range(0, mergeableGroups.Count)].ToList();
                Tower t1 = targetGroup[0];
                Tower t2 = targetGroup[1];
                int nextLank = t1.Lank + 1;

                if (nextLank <= 7)
                {
                    TileObject targetTile = t1.CurrentTile;
                    main.RemoveUnit(t1);
                    main.RemoveUnit(t2);
                    main.BuildTower(targetTile, nextLank);
                }
            }
        }
    }
}