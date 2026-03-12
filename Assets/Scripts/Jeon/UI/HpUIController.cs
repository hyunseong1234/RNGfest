using UnityEngine;
using System.Collections.Generic;

namespace Dev.jeon.UI
{
    public class HpUIController : MonoBehaviour
    {
        [Header("하트 아이콘 오브젝트들 (왼쪽부터 순서대로 넣으세요)")]
        [SerializeField] private List<GameObject> _heartIcons;

        /// <summary>
        /// 현재 HP에 맞춰 하트 UI를 켜고 끕니다.
        /// </summary>
        public void UpdateHpUI(int currentHp)
        {
            for (int i = 0; i < _heartIcons.Count; i++)
            {
                if (_heartIcons[i] != null)
                {
                    // i가 현재 체력보다 작으면 켜고(true), 크거나 같으면 끕니다(false)
                    // 예: currentHp가 2면, i가 0, 1일 때만 true가 됨
                    _heartIcons[i].SetActive(i < currentHp);
                }
            }
        }
    }
}