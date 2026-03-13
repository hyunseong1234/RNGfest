using TMPro;
using UnityEngine;

public class CurrentGoldText : MonoBehaviour
{
    [SerializeField] private TMP_Text _tmpText;

    public void UpdateGold(int amount, bool animate = true)
    {
        if (_tmpText == null) return;

        // 일단 지금은 연출 빼고 즉시 갱신으로 짜둘게요. 
        // 숫자가 안 올라가는 문제를 해결하는 게 우선이니까요!
        _tmpText.SetText("{0}", amount);
    }
}