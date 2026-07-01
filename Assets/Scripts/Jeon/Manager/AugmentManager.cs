using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Stats;
using Dev.jeon.Data;
using Dev.jeon.Model;
using Dev.jeon.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.jeon.Manager
{
    /// <summary>
    /// 증강 시스템 매니저
    /// - 보스 처치 시 증강 3개 뽑아서 UI에 표시
    /// - 가중치 기반 랜덤, 한번 나온 증강은 영구 제거
    /// - 선택한 타워 관련 증강만 풀에 포함
    /// </summary>
    public class AugmentManager : BaseManager
    {
        public static AugmentManager Instance { get; private set; }

        [Header("전체 증강 데이터 (SO 목록)")]
        [SerializeField] private List<AugmentData> _allAugments;

        // 현재 뽑기 가능한 증강 풀
        private List<AugmentData> _availableAugments = new List<AugmentData>();

        // 현재 플레이어가 보유 중인 증강 목록
        private List<AugmentData> _activeAugments = new List<AugmentData>();

        // 선택된 타워 타입 목록
        private List<TowerType> _selectedTowerTypes = new List<TowerType>();

        // 경제 증강 누적 값
        public int BonusGoldPerKill { get; private set; } = 0;
        public float InterestRate { get; private set; } = 0f;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 게임 시작 시 선택된 타워 목록 받아서 풀 초기화
        /// </summary>
        public void Init(List<TowerType> selectedTowers)
        {
            _selectedTowerTypes = selectedTowers;
            _activeAugments.Clear();
            BonusGoldPerKill = 0;
            InterestRate = 0f;

            // 선택한 타워 관련 증강 + 경제 + 타워 무관 궁극기만 풀에 포함
            _availableAugments = _allAugments.Where(a =>
                a.effectType == AugmentEffectType.Economy_Gold ||
                a.effectType == AugmentEffectType.Economy_Interest ||
                a.effectType == AugmentEffectType.Ultimate_SlowZone ||
                (a.targetTowerType != TowerType.None && _selectedTowerTypes.Contains(a.targetTowerType))
            ).ToList();

            Debug.Log($"[AugmentManager] 초기화 완료. 사용 가능한 증강 수: {_availableAugments.Count}");
        }

        /// <summary>
        /// 보스 처치 시 호출 → 증강 3개 뽑아서 UI에 전달
        /// </summary>
        /// 
        public void OnBossDefeated()
        {
            if (_availableAugments.Count == 0)
            {
                Debug.Log("[AugmentManager] 더 이상 뽑을 수 있는 증강이 없습니다.");
                return;
            }
            if (AugmentUI.Instance == null)
            {
                Debug.LogWarning("[AugmentManager] AugmentUI.Instance가 null입니다.");
                return;
            }
            // 이미 UI가 열려있으면 무시
            if (AugmentUI.Instance.IsOpen)
            {
                Debug.Log("[AugmentManager] 이미 증강 선택 중입니다.");
                return;
            }

            Time.timeScale = 0;
            List<AugmentData> picks = GetWeightedRandomAugments(3);
            AugmentUI.Instance.Show(picks);
            Debug.Log($"[AugmentManager] 증강 {picks.Count}개 뽑기 완료");
        }

        /// <summary>
        /// 가중치 기반으로 증강 n개 뽑기 (같은 회차 중복 없음)
        /// </summary>
        private List<AugmentData> GetWeightedRandomAugments(int count)
        {
            List<AugmentData> result = new List<AugmentData>();
            List<AugmentData> tempPool = new List<AugmentData>(_availableAugments);

            int pickCount = Mathf.Min(count, tempPool.Count);

            for (int i = 0; i < pickCount; i++)
            {
                int totalWeight = tempPool.Sum(a => a.weight);
                int roll = Random.Range(0, totalWeight);
                int current = 0;

                foreach (var augment in tempPool)
                {
                    current += augment.weight;
                    if (roll < current)
                    {
                        result.Add(augment);
                        tempPool.Remove(augment);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 플레이어가 증강 선택 시 호출 (AugmentUI에서 콜백으로 연결)
        /// </summary>
        public void OnAugmentSelected(AugmentData chosen)
        {
            ApplyAugment(chosen);
            _availableAugments.Remove(chosen);
            _activeAugments.Add(chosen);
            AugmentHUD.Instance?.AddAugmentSlot(chosen);
            Time.timeScale = 1;
            Debug.Log($"[AugmentManager] 증강 선택됨: {chosen.augmentName}");
        }

        /// <summary>
        /// 증강 효과 실제 적용
        /// </summary>
        private void ApplyAugment(AugmentData augment)
        {
            switch (augment.effectType)
            {
                case AugmentEffectType.StatModifier_Damage:
                    ApplyStatModifier(augment, StatTarget.Damage);
                    break;

                case AugmentEffectType.StatModifier_Speed:
                    ApplyStatModifier(augment, StatTarget.Speed);
                    break;

                case AugmentEffectType.AbilityBoost:
                    ApplyAbilityBoost(augment);
                    break;

                case AugmentEffectType.Economy_Gold:
                    BonusGoldPerKill += (int)augment.value;
                    Debug.Log($"[AugmentManager] 처치당 골드 +{augment.value}");
                    break;

                case AugmentEffectType.Economy_Interest:
                    InterestRate += augment.value;
                    Debug.Log($"[AugmentManager] 이자율 +{augment.value * 100}%");
                    break;

                case AugmentEffectType.Ultimate:
                    ApplyUltimate(augment);
                    break;

                case AugmentEffectType.Ultimate_SlowZone:
                    ApplySlowZone();
                    break;
            }
        }

        private enum StatTarget { Damage, Speed }

        private void ApplyStatModifier(AugmentData augment, StatTarget target)
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            if (main == null) return;

            foreach (var tower in main.SpawnTowers)
            {
                if (!tower.PoolTag.Contains(augment.targetTowerType.ToString())) continue;

                var mod = new StatModifier(augment.value, StatModType.Percent, this);

                if (target == StatTarget.Damage)
                    tower._stat.Damage.AddModifier(mod);
                else
                    tower._stat.Speed.AddModifier(mod);
            }

            Debug.Log($"[AugmentManager] {augment.targetTowerType} {target} +{augment.value * 100}%");
        }

        private void ApplyAbilityBoost(AugmentData augment)
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            if (main == null) return;

            foreach (var tower in main.SpawnTowers)
            {
                if (!tower.PoolTag.Contains(augment.targetTowerType.ToString())) continue;

                // IAbilityBoost 인터페이스로 다형성 처리
                // AttackTower → Bullet에 전달하는 방식으로 연결
                if (tower is AttackTower attackTower)
                {
                    if (attackTower.Bullet is IAbilityBoost boostable)
                        boostable.ApplyAbilityBoost(augment.value);
                }
            }

            Debug.Log($"[AugmentManager] {augment.targetTowerType} AbilityBoost +{augment.value}");
        }

        private void ApplyUltimate(AugmentData augment)
        {
            // TODO: UltimateManager에 해당 타워 궁극기 해금
            Debug.Log($"[AugmentManager] 궁극기 해금: {augment.targetTowerType}");
        }

        private void ApplySlowZone()
        {
            // TODO: 슬로우존 해금 처리
            Debug.Log("[AugmentManager] 슬로우존 해금");
        }

        public List<AugmentData> GetActiveAugments() => _activeAugments;

        public override void HandleEvent(string data) { }
    }
}