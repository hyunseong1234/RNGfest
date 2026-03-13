using Dev.cheol.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.cheol.UI
{
    public class UpgradeButton : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private int _upgradeIndex; // 0: 공격력, 1: 공속 등
        [SerializeField] private Button _btn;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _costText;

        private SystemManager _sys;

        private void Start()
        {
            _sys = ServiceLocator.Instance.GetService<SystemManager>();

            // 1. 버튼 클릭 이벤트 연결
            if (_btn != null)
                _btn.onClick.AddListener(OnClickUpgrade);

            // 2. 시스템 매니저의 신호를 구독 (골드 변경 시 버튼 활성/비활성 체크용)
            _sys.OnGoldChanged += RefreshButtonState;
            _sys.OnUpgradeChanged += (idx, lv) => { if (idx == _upgradeIndex) UpdateUI(); };

            // 3. 초기 UI 세팅
            UpdateUI();
        }

        private void OnClickUpgrade()
        {
            // 시스템 매니저에게 업그레이드 시도 요청
            _sys.TryUpgrade(_upgradeIndex);
        }

        private void UpdateUI()
        {
            if (_sys == null) return;

            int currentLevel = _sys.Upgrades[_upgradeIndex];

            // 레벨 텍스트 갱신 (박싱 방지 SetText)
            if (_levelText != null)
                _levelText.SetText("LV.{0}", currentLevel);

            // 비용 계산 (SystemManager의 로직과 동일하게)
            // 100, 150, 200... 등 형님이 정한 규칙대로 표시
            int nextCost = 100 * (currentLevel + 1);
            if (_costText != null)
                _costText.SetText("{0} G", nextCost);

            RefreshButtonState();
        }

        private void RefreshButtonState()
        {
            // 현재 골드가 부족하면 버튼을 비활성화(회색) 처리
            int nextCost = 100 * (_sys.Upgrades[_upgradeIndex] + 1);
            if (_btn != null)
                _btn.interactable = (_sys.Gold >= nextCost);
        }

        private void OnDestroy()
        {
            // 구독 해제
            if (_sys != null)
            {
                _sys.OnGoldChanged -= RefreshButtonState;
            }
        }
    }
}