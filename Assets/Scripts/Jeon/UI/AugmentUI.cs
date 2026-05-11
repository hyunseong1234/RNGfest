using Dev.jeon.Data;
using Dev.jeon.Manager;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Dev.jeon.UI
{
    /// <summary>
    /// 증강 선택 팝업 UI
    /// 카드 3개 고정 배치 방식
    /// </summary>
    public class AugmentUI : MonoBehaviour
    {
        public static AugmentUI Instance { get; private set; }

        [Header("UI 레퍼런스")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("증강 카드 3개")]
        [SerializeField] private AugmentCard _card1;
        [SerializeField] private AugmentCard _card2;
        [SerializeField] private AugmentCard _card3;

        private void Awake()
        {
            Instance = this;
            _panel?.SetActive(false);
        }

        /// <summary>
        /// 증강 선택 팝업 표시
        /// AugmentManager.OnBossDefeated()에서 호출
        /// </summary>
        public void Show(List<AugmentData> augments)
        {
            if (_titleText != null) _titleText.text = "증강을 선택하세요";

            if (augments.Count > 0) _card1?.Setup(augments[0], OnCardSelected);
            if (augments.Count > 1) _card2?.Setup(augments[1], OnCardSelected);
            if (augments.Count > 2) _card3?.Setup(augments[2], OnCardSelected);

            _panel?.SetActive(true);
        }

        // 카드 클릭 시 AugmentManager에 직접 전달
        private void OnCardSelected(AugmentData chosen)
        {
            AugmentManager.Instance.OnAugmentSelected(chosen);
            Hide();
        }

        public void Hide()
        {
            _panel?.SetActive(false);
            _card1?.Clear();
            _card2?.Clear();
            _card3?.Clear();
        }
    }
}