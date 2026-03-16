using Dev.cheol.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.cheol.UI
{
    public class UpgradeButton : MonoBehaviour
    {
        [Header("--- [슬롯 설정] ---")]
        [SerializeField] private int _mySlotIndex; // 0~4번

        [Header("--- [UI 컴포넌트] ---")]
        [SerializeField] private Image _towerIconImage; // 이놈만 그림 갈아끼움
        [SerializeField] private TMP_Text _lvText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _btn;

        private SystemManager _sys;
        private FactoryManager _factory;

        private void Start()
        {
            _sys = ServiceLocator.Instance.GetService<SystemManager>();
            _factory = ServiceLocator.Instance.GetService<FactoryManager>();

            if (_sys == null || _factory == null) return;

            _towerIconImage.sprite = _factory.PrefabSprite[_mySlotIndex];


            // 버튼 클릭 (슬롯 번호 전달)
            if (_btn != null)
                _btn.onClick.AddListener(() => _sys.TryUpgradeSlot(_mySlotIndex));

            // 실시간 UI 갱신 (골드/업그레이드 신호 감시)
            _sys.OnGoldChanged += RefreshUI;
            _sys.OnUpgradeChanged += (idx, lv) => { if (idx == _mySlotIndex) RefreshUI(); };

            RefreshUI();
        }



        private void RefreshUI()
        {
            if (_sys == null || _lvText == null || _costText == null || _btn == null) return;

            int lv = _sys.Upgrades[_mySlotIndex];
            int cost = _sys.GetCurrentUpgradeCost(_mySlotIndex);

            // 레벨 표시 (Lv.1, Lv.2...)
            _lvText.SetText("LV.{0}", lv + 1);

            // 비용 표시 및 버튼 활성화 체크
            if (cost == -1)
            {
                _costText.SetText("MAX");
                _btn.interactable = false;
            }
            else
            {
                _costText.SetText("{0}", cost);
                _btn.interactable = (_sys.Gold >= cost);
            }
        }

        private void OnDestroy()
        {
            if (_sys != null) _sys.OnGoldChanged -= RefreshUI;
        }
    }
}