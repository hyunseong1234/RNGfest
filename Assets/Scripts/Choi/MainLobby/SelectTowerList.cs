using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectTowerList : MonoBehaviour
{
    public static SelectTowerList Instance;
    public RectTransform DragLayer;
    public SelectTowerSlot[] _selectTowerSlot = null;

    private void Awake()
    {
        Instance = this;
        _selectTowerSlot = GetComponentsInChildren<SelectTowerSlot>(true);
    }
}



