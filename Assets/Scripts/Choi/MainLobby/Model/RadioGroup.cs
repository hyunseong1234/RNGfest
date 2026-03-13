using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.cheol.MainUI
{

    public class RadioGroup : MonoBehaviour
    {
        [SerializeField] private List<RadioBoxItem> radioItems;
        [SerializeField] private int defaultIndex = 0;
        public System.Action<int> OnChangedIndex; // 인덱스 변경 알림용 이벤트

        public int SelectedIndex { get; private set; }
        public int DefaultIndex { get => defaultIndex; set => defaultIndex = value; }

        private void Start()
        {
            // 초기 세팅: 아이템들에 인덱스 부여 및 클릭 리스너 연결
            for (int i = 0; i < radioItems.Count; i++)
            {
                int index = i;
                radioItems[i].Init(index, OnItemSelected);
            }
            defaultIndex = PlayFabDataManager.Instance.userData._currentSlot;
            // 기본 선택값 설정
            OnItemSelected(defaultIndex);
        }

        private void OnItemSelected(int index)
        {
            SelectedIndex = index;

            // 선택된 놈만 On, 나머지는 Off
            foreach (var item in radioItems)
            {
                item.SetState(item.Index == index);
            }

            // 외부(매니저)에 인덱스가 바뀌었다고 알려줌
            OnChangedIndex?.Invoke(index);
            Debug.Log($"[RadioBox] 현재 선택된 인덱스: {index}");
        }
    }
}