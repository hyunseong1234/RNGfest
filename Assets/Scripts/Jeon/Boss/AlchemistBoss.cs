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
        [SerializeField] private float _healPercent = 0.05f;      // 회복량 (최대 체력의 5%)
        [SerializeField] private float _skillDelay = 1.0f;
        [SerializeField] private float _mergeEffectDuration = 2.0f;

        [Header("Visual Effects")]
        [SerializeField] private BaseObject _mergeEffectPrefab;
        [SerializeField] private BaseObject _healEffectPrefab;    // 회복 시 잠깐 반짝이는 이펙트

        [Header("Sound Settings")]
        [SerializeField] private AudioClip _mergeSound;  // 합성 시 소리
        [SerializeField] private AudioClip _healSound;   // 회복 시 소리

        protected override IEnumerator ApplySkillEffectRoutine()
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            var pool = ServiceLocator.Instance.GetService<ObjectPoolingManger>();
            var sm = ServiceLocator.Instance.GetService<SoundManager>();

            if (main == null || pool == null || sm == null) yield break;

            // 1. 강제 합성 로직
            if (_mergeSound != null) sm.PlaySFX(_mergeSound);
            HandleForceMerge(main, pool);

            yield return new WaitForSeconds(_skillDelay);

            // 2. HP 회복 로직 (보호막 대신 체력 재생)
            if (_stat != null)
            {
                float healAmount = _stat.MaxHp.Value * _healPercent;

                // 현재 체력 회복 (최대 체력을 넘지 않도록 처리)
                _stat.CurrentHp = Mathf.Min(_stat.CurrentHp + healAmount, _stat.MaxHp.Value);

                // 사운드 및 이펙트 재생
                if (_healSound != null) sm.PlaySFX(_healSound);
                SpawnHealEffect(pool);

                // 화면에 회복 수치 표시
                ShowHealFont(pool, main, healAmount);

                Debug.Log($"[연금술사] {healAmount} HP 회복! (현재 HP: {_stat.CurrentHp})");
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

                SpawnMergeEffect(pool, t1.transform.position);
                SpawnMergeEffect(pool, t2.transform.position);

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

        // 힐 이펙트 생성 (보스 위치)
        private void SpawnHealEffect(ObjectPoolingManger pool)
        {
            if (_healEffectPrefab == null) return;

            var effect = pool.GetFromPool<BaseObject>(_healEffectPrefab);
            if (effect != null)
            {
                effect.gameObject.SetActive(true);
                effect.transform.position = transform.position; // 보스 위치에 생성
                StartCoroutine(ReturnEffectToPool(pool, effect, 1.5f));
            }
        }

        private void SpawnMergeEffect(ObjectPoolingManger pool, Vector3 pos)
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
            if (effect != null && effect.gameObject.activeSelf) pool.ReturnPool(effect);
        }

        private void ShowHealFont(ObjectPoolingManger pool, MainManager main, float amount)
        {
            var healFont = pool.GetFromPool<DamageFont>("DamageFont");
            if (healFont != null)
            {
                healFont.transform.localScale = Vector3.one * 3.0f;
                // Cyan 혹은 Green 컬러로 힐 느낌 강조
                healFont.SetDamage(amount, transform, FontColor.Cyan);
                main.SpawnUI.Add(healFont);
            }
        }
    }
}