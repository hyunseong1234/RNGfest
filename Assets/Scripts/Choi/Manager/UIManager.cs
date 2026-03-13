using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{
    public class UIManager : UpdateManager
    {
        [SerializeField] private CurrentGoldText _goldText;

        private void Start()
        {
            // 1. 시스템 매니저를 가져온다
            var sys = ServiceLocator.Instance.GetService<SystemManager>();

            if (sys != null && _goldText != null)
            {
                // 2. 골드가 바뀌면 실행할 함수를 등록 (람다식)
                sys.OnGoldChanged += () =>
                {
                    _goldText.UpdateGold(sys.Gold);
                };

                // 3. 게임 시작하자마자 현재 골드로 초기화
                _goldText.UpdateGold(sys.Gold, false);
            }
        }

        public override void HandleEvent(string eventName) { }
        public override void ManagerUpdate() { }
    }
}