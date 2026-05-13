using Dev.jeon.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dev.jeon.UI
{
    /// <summary>
    /// 보유 증강 슬롯
    /// 평상시: 아이콘만 표시
    /// 마우스 오버: 툴팁 (이름 + 설명) 표시
    /// </summary>
    public class AugmentSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("슬롯 UI")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;

        [Header("툴팁 UI")]
        [SerializeField] private GameObject _tooltip;
        [SerializeField] private TextMeshProUGUI _tooltipName;
        [SerializeField] private TextMeshProUGUI _tooltipDescription;

        [Header("타입별 색상")]
        [SerializeField] private Color _towerBuffColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color _economyColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color _ultimateColor = new Color(0.8f, 0.2f, 1f);

        private void Awake()
        {
            // 시작 시 툴팁 숨기기
            _tooltip?.SetActive(false);
        }

        public void Setup(AugmentData data)
        {
            // 아이콘 설정
            if (_iconImage != null)
            {
                _iconImage.sprite = data.icon;
                _iconImage.enabled = data.icon != null;
            }

            // 배경 색상 설정
            if (_backgroundImage != null)
                _backgroundImage.color = GetColorByType(data.effectType);

            // 툴팁 텍스트 설정
            if (_tooltipName != null) _tooltipName.text = data.augmentName;
            if (_tooltipDescription != null) _tooltipDescription.text = data.description;
        }

        /// <summary>
        /// 마우스 올리면 툴팁 표시
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            _tooltip?.SetActive(true);
        }

        /// <summary>
        /// 마우스 나가면 툴팁 숨기기
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltip?.SetActive(false);
        }

        private Color GetColorByType(AugmentEffectType type)
        {
            switch (type)
            {
                case AugmentEffectType.Economy_Gold:
                case AugmentEffectType.Economy_Interest:
                    return _economyColor;
                case AugmentEffectType.Ultimate:
                case AugmentEffectType.Ultimate_SlowZone:
                    return _ultimateColor;
                default:
                    return _towerBuffColor;
            }
        }
    }
}