using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{

    public class SystemManager : BaseManager
    {
        [SerializeField] private int _gold = 100; //골드
        [SerializeField] private float _gameTime = 1; // 게임 배속
        [SerializeField] private int[] upgrades = new int[5]; //타워 강화 상태
        [SerializeField] private int _buildCount = 0; //타워 뽑기에 해당되는 변수
        [SerializeField] private int _life = 3; //생명력
        [SerializeField] private int _currentWave = 0;// 현재 웨이브
        [SerializeField] private int _max_Wave = 50; //마지막 웨이브


        // 알람(이벤트) 하나 만듭니다.
        public Action OnBuildCountChanged;

        // 프로퍼티를 수정해서, 값이 바뀔 때마다 알람을 울리게(Invoke) 합니다.



        #region 프로퍼티
        public int Gold { get => _gold; set => _gold = value; }
        public float GameTime { get => _gameTime; set => _gameTime = value; }
        public int[] Upgrades { get => upgrades; set => upgrades = value; }
        // 골드 증가를 기록하기 위해서 추가
        public int BuildCount
        {
            get => _buildCount;
            set
            {
                _buildCount = value;
                OnBuildCountChanged?.Invoke(); // "타워 지어졌다! UI 업데이트 해라!" 알람
            }
        }
        public int Life { get => _life; set => _life = value; }
        public int CurrentWave { get => _currentWave; set => _currentWave = value; }
        public int Max_Wave { get => _max_Wave; set => _max_Wave = value; }

        public override void HandleEvent(string data)
        {
            throw new System.NotImplementedException();
        }
        //증강 Info
        #endregion
    }

}
