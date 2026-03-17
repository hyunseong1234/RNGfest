using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageSlotBar : MonoBehaviour
{
    public Image _thisimage;
    public TMP_Text _text;
    public void SetSlotColor(bool isActive)
    {
        // isActive가 true면 밝은색, false면 어두운 회색 등으로 변경
        _thisimage.color = isActive ? Color.yellow : Color.gray;
    }

    public void SetAbility(int index)
    {
        int totalBonus = (index - 1) * 5;
        _text.SetText($"공격력 <color=#FFD700>{totalBonus}%</color> 증가");
    }
}
