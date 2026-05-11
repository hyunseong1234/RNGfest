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
        [SerializeField] private Color _towerBuffColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color _economyColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color _ultimateColor = new Color(0.8f, 0.2f, 1f);

        private AugmentData _data;
        private System.Action<AugmentData> _onSelected;

        private void Awake()
        {
            _button?.onClick.AddListener(OnCardClicked);
        }

        public void Setup(AugmentData data, System.Action<AugmentData> onSelected)
        {
            _data = data;
            _onSelected = onSelected;

            if (_nameText != null) _nameText.text = data.augmentName;
            if (_descriptionText != null) _descriptionText.text = data.description;

            if (_iconImage != null)
            {
                _iconImage.sprite = data.icon;
                _iconImage.enabled = data.icon != null;
            }

            if (_backgroundImage != null)
                _backgroundImage.color = GetColorByType(data.effectType);
        }

        /// <summary>
        /// 카드 초기화 - Hide() 시 호출
        /// </summary>
        public void Clear()
        {
            _data = null;
            _onSelected = null;
            if (_nameText != null) _nameText.text = "";
            if (_descriptionText != null) _descriptionText.text = "";
            if (_iconImage != null) _iconImage.sprite = null;
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
                default:
                    return _towerBuffColor;
            }
        }

        private void OnDestroy()
        {
            _button?.onClick.RemoveListener(OnCardClicked);
        }
    }
}