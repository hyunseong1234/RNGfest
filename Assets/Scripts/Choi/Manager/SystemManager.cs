using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{
    enum UpgradeStat
    {
        None = 0,
        Damage = 1,
        Range = 2,
        Speed = 3,
    }
    public class SystemManager : BaseManager
    {
        [SerializeField] private int _gold = 100; //골드
        [SerializeField] private float _gameTime = 1; // 게임 배속
        [SerializeField] private int[] upgrades = new int[5]; //타워 강화 상태
        [SerializeField] private int _buildCount = 0; //타워 뽑기에 해당되는 변수
        [SerializeField] private int _life = 3; //생명력
        [SerializeField] private int _currentWave = 0;// 현재 웨이브
        [SerializeField] private int _max_Wave = 50; //마지막 웨이브
        private int[] _upgradeCosts = { 100, 200, 400, 800, 1600 }; //업그레이드 비용

        public Action OnGoldChanged;

        public Action OnBuildCountChanged;

        // 프로퍼티를 수정해서, 값이 바뀔 때마다 알람을 울리게(Invoke) 합니다.

        #region 프로퍼티
        public int Gold
        {
            get => _gold;
            set
            {
                _gold = value;
                // 골드가 바뀔 때마다 "골드 변했다!"라고 알림을 울림
                OnGoldChanged?.Invoke();
            }
        }
        public float GameTime { get => _gameTime; set => _gameTime = value; }
        public int[] Upgrades { get => upgrades; set => upgrades = value; }
        // 골드 증가를 기록하기 위해서 추가
        public int BuildCount
        {
            get => _buildCount;
            set
            {
                _buildCount = value;
                OnBuildCountChanged?.Invoke();
            }
        }
        public int Life { get => _life; set => _life = value; }
        public int CurrentWave { get => _currentWave; set => _currentWave = value; }
        public int Max_Wave { get => _max_Wave; set => _max_Wave = value; }

        public Action<int, int> OnUpgradeChanged;


        public void TryUpgrade(int index)
        {
            if (index < 0 || index >= upgrades.Length) return;

            int cost = _upgradeCosts[index] * (upgrades[index] + 1); // 레벨당 비용 증가 로직
            if (upgrades[index] >= 5) return; // 최대 5강 제한

            if (Gold >= cost)
            {
                Gold -= cost; // 프로퍼티 set 실행 -> UI 자동 갱신
                upgrades[index]++;

                OnUpgradeChanged?.Invoke(index, upgrades[index]);
                Debug.Log($"[Upgrade] {index}번 업그레이드 완료. 현재 레벨: {upgrades[index]}");
            }
            else
            {
                Debug.Log("골드가 부족합니다!");
                // 여기서 UIManager를 통해 "돈 부족" 팝업을 띄울 수도 있음
            }
        }

        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }
        //증강 Info
        #endregion
    }

}
