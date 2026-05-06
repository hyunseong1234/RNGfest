using Dev.jeon.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.jeon.UI
{
    /// <summary>
    /// 증강 선택 카드 하나
    /// 이름 + 설명 + 아이콘 표시
    /// 클릭 시 AugmentUI에 콜백
    /// </summary>
    public class AugmentCard : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Button _button;

        [Header("타입별 색상")]
        [SerializeField] private Color _towerBuffColor = new Color(0.2f, 0.6f, 1f);    // 파랑
        [SerializeField] private Color _economyColor = new Color(1f, 0.8f, 0.2f);    // 노랑
        [SerializeField] private Color _ultimateColor = new Color(0.8f, 0.2f, 1f);    // 보라

        private AugmentData _data;
        private System.Action<AugmentData> _onSelected;

        private void Awake()
        {
            _button?.onClick.AddListener(OnCardClicked);
        }

        /// <summary>
        /// 카드 초기화
        /// </summary>
        public void Setup(AugmentData data, System.Action<AugmentData> onSelected)
        {
            _data = data;
            _onSelected = onSelected;

            // 텍스트 설정
            if (_nameText != null) _nameText.text = data.augmentName;
            if (_descriptionText != null) _descriptionText.text = data.description;

            // 아이콘 설정
            if (_iconImage != null)
            {
                _iconImage.sprite = data.icon;
                _iconImage.enabled = data.icon != null;
            }

            // 타입별 배경 색상
            if (_backgroundImage != null)
                _backgroundImage.color = GetColorByType(data.effectType);
        }

        private void OnCardClicked()
        {
            _onSelected?.Invoke(_data);
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

                default: // StatModifier, AbilityBoost
                    return _towerBuffColor;
            }
        }

        private void OnDestroy()
        {
            _button?.onClick.RemoveListener(OnCardClicked);
        }
    }
}