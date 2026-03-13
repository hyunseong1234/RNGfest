using Dev.cheol.MainUI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TowerPresetManager : MonoBehaviour
{
    public static TowerPresetManager Instance;
    [SerializeField] private RadioGroup _presetRadioGroup;
    [SerializeField] private List<SelectTowerSlot> _teamSlots; // 5개 슬롯
    [SerializeField] private LobbyInventoryUI _inventoryUI;
    [SerializeField] private TMP_Text _slotText = null;

    private void Awake() => Instance = this;

    private void Start()
    {

        // 라디오 버튼 선택 시 -> 서버 데이터 인덱스 변경 -> UI 갱신 (끝)
        _presetRadioGroup.OnChangedIndex += (index) =>
        {
            PlayFabDataManager.Instance.userData._currentSlot = index;
            RefreshAll();
            PlayFabDataManager.Instance.SaveData();
        };

        if (PlayFabDataManager.Instance?.userData != null) RefreshAll();
        _slotText.SetText("{0}", PlayFabDataManager.Instance.userData._currentSlot + 1);
    }

    public void RefreshAll()
    {
        //임시 치트키
        _slotText.SetText("{0}", PlayFabDataManager.Instance.userData._currentSlot + 1);
        foreach (var slot in _teamSlots) slot.RefreshStoredData();
        if (_inventoryUI != null) _inventoryUI.RefreshInventory();
    }
}