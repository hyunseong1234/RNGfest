using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// IPointerClickHandler 인터페이스를 추가합니다.
public class SelectTowerSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    // [가장 중요] 하이라키에서 각 슬롯마다 0, 1, 2, 3, 4가 직접 입력되어 있어야 함
    public int slotIndex;

    [Header("UI Components")]
    [SerializeField] private Image _towerIconImage;

    private void Start()
    {
        RefreshStoredData();
    }

    // 프리셋이 바뀌거나 드롭될 때마다 호출됨
    public void RefreshStoredData()
    {
        var userData = PlayFabDataManager.Instance?.userData;
        if (userData == null) return;

        int currentDeckIdx = userData._currentSlot;
        var towerList = userData._towerSlots[currentDeckIdx].slotTowers;

        while (towerList.Count <= slotIndex)
        {
            towerList.Add(TowerType.None);
        }

        TowerType myTowerType = towerList[slotIndex];
        UpdateSlotUI(myTowerType);
    }

    public void UpdateSlotUI(TowerType type)
    {
        if (_towerIconImage == null) return;

        // 타워가 없으면(None) 아이콘을 끄거나 투명하게 처리
        if (type == TowerType.None)
        {
            _towerIconImage.gameObject.SetActive(false);
            return;
        }

        Sprite towerSprite = TowerSlotManager.Instance.GetTowerSprite(type);
        if (towerSprite != null)
        {
            _towerIconImage.gameObject.SetActive(true);
            _towerIconImage.sprite = towerSprite;
        }
    }

    // 드래그 앤 드롭으로 장착
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        var draggedItem = eventData.pointerDrag.GetComponent<SelectFramImage>();
        if (draggedItem != null)
        {
            Debug.Log($"[Drop 발생] 내 슬롯 번호: {slotIndex} | 넣으려는 타워: {draggedItem.GetTowerType()}");

            TowerSlotManager.Instance.ChangeTowerInSlot(slotIndex, draggedItem.GetTowerType());
            TowerPresetManager.Instance.RefreshAll();
        }
    }

    // 더블클릭 시 장착 해제
    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭 횟수가 2회(더블클릭)일 때만 작동
        if (eventData.clickCount == 2)
        {
            Debug.Log($"[더블클릭] {slotIndex}번 슬롯 해제");

            // 해당 슬롯을 None으로 변경
            TowerSlotManager.Instance.ChangeTowerInSlot(slotIndex, TowerType.None);

            // 전체 UI 새로고침 (이때 인벤토리의 해당 타워는 SetActive(true)가 됨)
            TowerPresetManager.Instance.RefreshAll();
        }
    }
}