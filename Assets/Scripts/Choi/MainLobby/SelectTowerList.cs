using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectTowerList : MonoBehaviour
{
    public static SelectTowerList Instance;
    public RectTransform DragLayer;
    public List<SelectTowerSlot> _selectTowerSlot = null;

    private void Awake()
    {
        Instance = this;
        _selectTowerSlot = GetComponentsInChildren<SelectTowerSlot>(true).ToList();

        for (int i = 0; i < _selectTowerSlot.Count; i++)
        {
            _selectTowerSlot[i].slotIndex = i;

        }
    }
    private void OnEnable()
    {
        if (TowerPresetManager.Instance != null) TowerPresetManager.Instance.RefreshAll();
    }
}



