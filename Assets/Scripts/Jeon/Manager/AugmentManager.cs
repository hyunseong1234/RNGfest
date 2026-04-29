using Dev.cheol.Data;
using Dev.cheol.Model;
using Dev.cheol.Stats;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.cheol.Manager
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

        // 현재 뽑기 가능한 증강 풀 (이미 나온 건 제거됨)
        private List<AugmentData> _availableAugments = new List<AugmentData>();

        // 현재 플레이어가 보유 중인 증강 목록
        private List<AugmentData> _activeAugments = new List<AugmentData>();

        // 선택된 타워 타입 목록 (게임 시작 시 세팅됨)
        private List<TowerType> _selectedTowerTypes = new List<TowerType>();

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 게임 시작 시 FactoryManager에서 선택된 타워 목록 받아서 풀 초기화
        /// </summary>
        public void Init(List<TowerType> selectedTowers)
        {
            _selectedTowerTypes = selectedTowers;
            _activeAugments.Clear();

            // 선택한 타워 관련 증강 + 경제/궁극기 증강만 풀에 포함
            _availableAugments = _allAugments.Where(a =>
                a.augmentType == AugmentType.Economy ||
                a.augmentType == AugmentType.Ultimate ||
                (a.augmentType == AugmentType.TowerBuff && _selectedTowerTypes.Contains(a.targetTowerType))
            ).ToList();

            Debug.Log($"[AugmentManager] 초기화 완료. 사용 가능한 증강 수: {_availableAugments.Count}");
        }

        /// <summary>
        /// 보스 처치 시 호출 → 증강 3개 뽑아서 UI에 전달
        /// </summary>
        public void OnBossDefeated()
        {
            if (_availableAugments.Count == 0)
            {
                Debug.Log("[AugmentManager] 더 이상 뽑을 수 있는 증강이 없습니다.");
                return;
            }

            // 게임 일시정지
            Time.timeScale = 0;

            List<AugmentData> picks = GetWeightedRandomAugments(3);
            // TODO: AugmentUI.Instance.Show(picks, OnAugmentSelected);
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

            // 영구 제거 (다시는 안 나옴)
            _availableAugments.Remove(chosen);
            _activeAugments.Add(chosen);

            // 게임 재개
            Time.timeScale = 1;

            Debug.Log($"[AugmentManager] 증강 선택됨: {chosen.augmentName}");
        }

        /// <summary>
        /// 증강 효과 실제 적용
        /// </summary>
        private void ApplyAugment(AugmentData augment)
        {
            switch (augment.augmentType)
            {
                case AugmentType.TowerBuff:  ApplyTowerBuff(augment); break;
                case AugmentType.Economy:    ApplyEconomy(augment);   break;
                case AugmentType.Ultimate:   ApplyUltimate(augment);  break;
            }
        }

        private void ApplyTowerBuff(AugmentData augment)
        {
            var main = ServiceLocator.Instance.GetService<MainManager>();
            if (main == null) return;

            foreach (var tower in main.SpawnTowers)
            {
                // 기존 Tower.cs의 PoolTag 방식 그대로 사용
                if (!tower.PoolTag.Contains(augment.targetTowerType.ToString())) continue;

                var mod = new StatModifier(augment.value, StatModType.Percent, this);

                switch (augment.statType)
                {
                    case AugmentStatType.Damage: tower._stat.Damage.AddModifier(mod); break;
                    case AugmentStatType.Speed:  tower._stat.Speed.AddModifier(mod);  break;
                    case AugmentStatType.Range:  tower._stat.Range.AddModifier(mod);  break;
                }
            }

            Debug.Log($"[AugmentManager] 타워 버프 적용: {augment.targetTowerType} {augment.statType} +{augment.value * 100}%");
        }

        private void ApplyEconomy(AugmentData augment)
        {
            // TODO: SystemManager에 골드 보너스 등록
            Debug.Log($"[AugmentManager] 경제 증강 적용: 처치당 +{augment.bonusGoldPerKill} 골드");
        }

        private void ApplyUltimate(AugmentData augment)
        {
            // TODO: 해당 타워의 궁극기 해금 처리
            Debug.Log($"[AugmentManager] 궁극기 해금: {augment.ultimateTowerType}");
        }

        /// <summary>
        /// 현재 보유 증강 목록 반환 (UI 표시용)
        /// </summary>
        public List<AugmentData> GetActiveAugments() => _activeAugments;

        public override void HandleEvent(string data) { }
    }
}
