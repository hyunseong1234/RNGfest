using UnityEngine;
using UnityEngine.EventSystems;

public class SelectTowerSlot : MonoBehaviour, IDropHandler
{
    public int slotIndex; // 슬롯 번호

    public void OnDrop(PointerEventData eventData)
    {
        // 드래그 중인 아이콘을 찾아옵니다.
        if (eventData.pointerDrag != null)
        {
            // 여기서 아이콘의 위치를 이 슬롯의 정중앙으로 고정시키거나,
            // 타워 데이터를 교체하는 로직을 작성합니다.
            Debug.Log($"{slotIndex}번 슬롯에 아이콘이 드롭되었습니다!");

            // 예: 드래그하던 아이콘의 부모를 이 슬롯으로 변경
            eventData.pointerDrag.transform.SetParent(this.transform);
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}