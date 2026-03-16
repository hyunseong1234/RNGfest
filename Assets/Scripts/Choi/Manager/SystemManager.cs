using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{
    // [추가] 랜덤다이스 스타일 업그레이드 타입 정의
    public enum UpgradeStat
    {
        None = 0,
        Damage = 1,
        Range = 2,
        Speed = 3,
        // 필요에 따라 더 추가 (예: Electric, Poison 등)
    }

    public class SystemManager : BaseManager
    {
        [Header("--- 원본 데이터 (보존) ---")]
        [SerializeField] private int _gold = 100; // 골드
        [SerializeField] private float _gameTime = 1; // 게임 배속
        [SerializeField] private int[] upgrades = new int[5]; // 타워 강화 상태
        [SerializeField] private int _buildCount = 0; // 타워 뽑기에 해당되는 변수
        [SerializeField] private int _life = 3; // 생명력
        [SerializeField] private int _currentWave = 0; // 현재 웨이브
        [SerializeField] private int _max_Wave = 50; // 마지막 웨이브

        [Header("--- 업그레이드 설정 ---")]
        // 랜덤다이스처럼 레벨마다 들어가는 고정 비용 테이블
        private int[] _upgradeCosts = { 100, 200, 400, 800, 1600 };

        [Header("--- 알람(이벤트) 리스트 ---")]
        public Action OnGoldChanged;
        public Action OnBuildCountChanged;
        public Action<int, int> OnUpgradeChanged; // 업그레이드 시 타워들에게 보내는 신호

        #region 프로퍼티 (형님 원본 + 로직 추가)

        public int Gold
        {
            get => _gold;
            set
            {
                _gold = value;
                OnGoldChanged?.Invoke(); // UI 자동 갱신
            }
        }

        public float GameTime { get => _gameTime; set => _gameTime = value; }

        public int[] Upgrades { get => upgrades; set => upgrades = value; }

        public int BuildCount
        {
            get => _buildCount;
            set
            {
                _buildCount = value;
                OnBuildCountChanged?.Invoke(); // 타워 건설 알람
            }
        }

        public int Life { get => _life; set => _life = value; }
        public int CurrentWave { get => _currentWave; set => _currentWave = value; }
        public int Max_Wave { get => _max_Wave; set => _max_Wave = value; }

        #endregion

        #region 핵심 기능 로직

        /// <summary>
        /// 랜덤다이스 스타일: 버튼 클릭 시 호출되는 업그레이드 함수
        /// </summary>
        // SystemManager.cs 내부 핵심 로직
        public void TryUpgradeSlot(int slotIndex)
        {
            // slotIndex: 0~4 (내 덱의 5개 슬롯)
            if (slotIndex < 0 || slotIndex >= upgrades.Length) return;

            int currentLv = upgrades[slotIndex];
            if (currentLv >= 5) return; // 만렙 체크

            int cost = _upgradeCosts[currentLv];

            if (Gold >= cost)
            {
                Gold -= cost;
                upgrades[slotIndex]++; // 해당 슬롯 유닛의 전역 레벨 상승

                // 알람: "n번 슬롯 유닛 레벨업했다!"
                OnUpgradeChanged?.Invoke(slotIndex, upgrades[slotIndex]);
            }
        }

        // 현재 단계의 비용을 UI에 표시하기 위해 반환하는 함수
        public int GetCurrentUpgradeCost(int index)
        {
            if (index < 0 || index >= upgrades.Length) return -1;
            if (upgrades[index] >= 5) return -1; // 만렙 시 -1 반환
            return _upgradeCosts[upgrades[index]];
        }

        public override void HandleEvent(string data)
        {
            switch (data)
            {
                case "WaveStart":
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }

        #endregion
    }
}