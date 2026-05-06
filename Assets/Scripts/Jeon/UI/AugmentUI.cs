using Dev.jeon.Data;
using Dev.jeon.Manager;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Dev.jeon.UI
{
    /// <summary>
    /// 증강 선택 팝업 UI
    /// - 보스 처치 시 AugmentManager에서 Show() 호출
    /// - 3개 카드 표시 후 선택 시 AugmentManager.OnAugmentSelected() 호출
    /// </summary>
    public class AugmentUI : MonoBehaviour
    {
        public static AugmentUI Instance { get; private set; }

        [Header("UI 레퍼런스")]
        [SerializeField] private GameObject _panel;           // 전체 팝업 패널
        [SerializeField] private Transform _cardContainer;    // 카드 3개 들어갈 부모
        [SerializeField] private AugmentCard _cardPrefab;     // 카드 프리팹
        [SerializeField] private TextMeshProUGUI _titleText;  // "증강을 선택하세요" 텍스트

        // 현재 표시 중인 카드 목록
        private List<AugmentCard> _activeCards = new List<AugmentCard>();

        private void Awake()
        {
            Instance = this;
            _panel?.SetActive(false);
        }

        /// <summary>
        /// 증강 선택 팝업 표시
        /// AugmentManager.OnBossDefeated()에서 호출
        /// </summary>
        public void Show(List<AugmentData> augments, System.Action<AugmentData> onSelected)
        {
            // 기존 카드 정리
            ClearCards();

            // 카드 생성
            foreach (var augment in augments)
            {
                var card = Instantiate(_cardPrefab, _cardContainer);
                card.Setup(augment, (chosen) =>
                {
                    onSelected?.Invoke(chosen);
                    Hide();
                });
                _activeCards.Add(card);
            }

            // 패널 표시
            if (_titleText != null) _titleText.text = "증강을 선택하세요";
            _panel?.SetActive(true);
        }

        /// <summary>
        /// 팝업 닫기
        /// </summary>
        public void Hide()
        {
            _panel?.SetActive(false);
            ClearCards();
        }

        private void ClearCards()
        {
            foreach (var card in _activeCards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            _activeCards.Clear();
        }
    }
}