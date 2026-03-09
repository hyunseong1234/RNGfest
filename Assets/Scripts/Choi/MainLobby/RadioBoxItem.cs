using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.cheol.MainUI
{
    public class RadioBoxItem : MonoBehaviour
    {
        [SerializeField] private GameObject _activeIndicator; // 활성화 시 보여줄 오브젝트 (예: 체크 이미지)
        [SerializeField] private Button _targetButton;
        [SerializeField] private TMP_Text _text;

        public int Index { get; private set; }
        private Action<int> _onSelect;

        public void Init(int index, Action<int> onSelectAction)
        {
            Index = index;
            _onSelect = onSelectAction;

            if (_targetButton == null) _targetButton = GetComponent<Button>();
            _targetButton.onClick.AddListener(() => _onSelect?.Invoke(Index));

            if (_text == null) _text = GetComponent<TMP_Text>();
            _text.SetText("{0}", index);
        }

        public void SetState(bool isActive)
        {
            Debug.Log("라디오 박스 실행중");
            if (_activeIndicator != null)
            {
                _activeIndicator.SetActive(isActive);
            }
        }
    }

}
