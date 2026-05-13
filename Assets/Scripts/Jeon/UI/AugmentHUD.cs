using Dev.jeon.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.jeon.UI
{
    /// <summary>
    /// 화면 하단 보유 증강 표시 HUD
    /// 증강 선택 시 자동으로 슬롯 추가
    /// </summary>
    public class AugmentHUD : MonoBehaviour
    {
        public static AugmentHUD Instance { get; private set; }

        [Header("슬롯 설정")]
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private AugmentSlot _slotPrefab;

        private List<AugmentSlot> _activeSlots = new List<AugmentSlot>();

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 증강 선택 시 호출 → 슬롯 추가
        /// AugmentManager.OnAugmentSelected()에서 호출
        /// </summary>
        public void AddAugmentSlot(AugmentData data)
        {
            var slot = Instantiate(_slotPrefab, _slotContainer);
            slot.Setup(data);
            _activeSlots.Add(slot);
        }

        /// <summary>
        /// 게임 재시작 시 초기화
        /// </summary>
        public void Clear()
        {
            foreach (var slot in _activeSlots)
                if (slot != null) Destroy(slot.gameObject);
            _activeSlots.Clear();
        }
    }
}