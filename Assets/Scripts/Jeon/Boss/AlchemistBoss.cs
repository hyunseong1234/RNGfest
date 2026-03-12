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
        [SerializeField] private float _shieldPercent = 0.05f;
        [SerializeField] private float _skillDelay = 1.0f;
        [SerializeField] private float _mergeEffectDuration = 2.0f;

        [Header("Visual Effects (Direct Prefab)")]
        [SerializeField] private BaseObject _mergeEffectPrefab;   // 합성 즉발 연출 프리팹
        [SerializeField] private BaseObject _shieldEffectPrefab;  // 루프형 보호막 프리팹
        [SerializeField] private BaseObject _shatterEffectPrefab; // 보호막 파괴 연출 프리팹

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();

            if (main == null || pool == null) yield break;

            // 1. 강제 합성 로직 (즉발 연출)
            HandleForceMerge(main, pool);

            // 2. 1초 대기
            yield return new WaitForSeconds(_skillDelay);

            // 3. 보호막 생성 로직
            if (_stat != null)
            {
                float shieldAmount = _stat.MaxHp.Value * _shieldPercent;

                // [수정] 프리팹 객체 자체를 넘겨줍니다.
                AddShield(shieldAmount, _shieldEffectPrefab, _shatterEffectPrefab);

                ShowHealFont(pool, main, shieldAmount);
                Debug.Log($"[연금술사] {shieldAmount} 보호막 생성!");
            }
        }

        private void HandleForceMerge(MainManager main, ObjectPoolingManger pool)
        {
            var activeTowers = main.SpawnTowers.Where(t => t != null && t.gameObject.activeSelf).ToList();
            var mergeableGroups = activeTowers.GroupBy(t => t.Lank).Where(g => g.Count() >= 2).ToList();

            if (mergeableGroups.Count > 0)
            {
                var targetGroup = mergeableGroups[UnityEngine.Random.Range(0, mergeableGroups.Count)].ToList();
                Tower t1 = targetGroup[0];
                Tower t2 = targetGroup[1];

                // [연출] 프리팹 직접 전달
                SpawnEffect(pool, t1.transform.position);
                SpawnEffect(pool, t2.transform.position);

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

        private void SpawnEffect(ObjectPoolingManger pool, Vector3 pos)
        {
            if (_mergeEffectPrefab == null) return;

            var effect = pool.GetFromPool<BaseObject>(_mergeEffectPrefab);
            if (effect != null)
            {
                effect.gameObject.SetActive(true);
                effect.transform.position = pos + Vector3.up * 0.5f;
                StartCoroutine(ReturnEffectToPool(pool, effect, _mergeEffectDuration));
            }
        }
        private IEnumerator ReturnEffectToPool(ObjectPoolingManger pool, BaseObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (effect != null && effect.gameObject.activeSelf)
            {
                // GetComponent 없이 받은 객체 그대로 반납
                pool.ReturnPool(effect);
            }
        }

        private void ShowHealFont(ObjectPoolingManger pool, MainManager main, float amount)
        {
            var shieldFont = pool.GetFromPool<DamageFont>("DamageFont");
            if (shieldFont != null)
            {
                shieldFont.transform.localScale = Vector3.one * 3.0f;
                shieldFont.SetDamage(amount, transform, FontColor.Cyan);
                main.SpawnUI.Add(shieldFont);
            }
        }
    }
}