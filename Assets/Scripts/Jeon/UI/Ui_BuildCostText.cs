using UnityEngine;
using TMPro;
using Dev.cheol.Manager;
using System;

namespace Dev.jeon.UI
{
    public class Ui_BuildCostText : UIObject
    {
        [SerializeField]private TextMeshProUGUI costText;
        private void Start()
        {
            var system = ServiceLocator.Instance.GetService<SystemManager>();
            if (system != null)
            {
                // 1. 매니저의 알람 방송국을 구독합니다. 
                // (BuildCount가 바뀔 때마다 updateCostUI 함수가 자동으로 실행됨)
                system.OnBuildCountChanged += updateCostUI;
            }

            // 2. 게임 시작 시 최초 1회 업데이트
            updateCostUI();
        }

        private void updateCostUI()
        {
            var system = ServiceLocator.Instance.GetService<SystemManager>();
            if (system == null || costText == null) return;

            int needGold = 10 + (system.BuildCount * 10);
            costText.text = $"Tower {needGold}G";
        }

        // 주의: 스크립트가 파괴될 때는 알람 구독을 취소해야 메모리 누수가 없습니다.
        private void OnDestroy()
        {
            if (ServiceLocator.Instance == null) return;

            var system = ServiceLocator.Instance.GetService<SystemManager>();
            if (system != null)
            {
                system.OnBuildCountChanged -= updateCostUI; // 구독 취소
            }
        }
    }
}

