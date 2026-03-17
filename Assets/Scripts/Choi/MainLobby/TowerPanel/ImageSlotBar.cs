using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSlotBar : MonoBehaviour
{
    public Image _thisimage;

    public void SetSlotColor(bool isActive)
    {
        // isActive가 true면 밝은색, false면 어두운 회색 등으로 변경
        _thisimage.color = isActive ? Color.yellow : Color.gray;
    }
}
