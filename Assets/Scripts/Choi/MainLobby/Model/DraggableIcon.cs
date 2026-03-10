using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableIcon : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private int originalIndex;
    private Vector3 initialScale;

    [Header("Settings")]
    public float scaleMultiplier = 1.2f;

    void Awake()
    {
        // TryGetComponent로 안전하고 빠르게 캐싱
        if (!TryGetComponent(out rectTransform))
            Debug.LogError($"{gameObject.name}: RectTransform이 없습니다!");

        if (!TryGetComponent(out canvasGroup))
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        initialScale = rectTransform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. 복귀를 위한 현재 상태 저장
        originalParent = transform.parent;
        originalIndex = transform.GetSiblingIndex();

        // 2. 드래그 전용 최상위 부모로 이동 (SelectTowerList의 정적 인스턴스 활용)
        // SelectTowerList에 public Transform dragLayer가 있다고 가정합니다.
        if (SelectTowerList.Instance != null && SelectTowerList.Instance.DragLayer != null)
        {
            transform.SetParent(SelectTowerList.Instance.DragLayer);
        }
        else
        {
            // 드래그 레이어가 없다면 하이라키 최하단으로 보내서 우선순위 확보
            transform.SetAsLastSibling();
        }

        // 3. 클릭 피드백 (커지기 & 투명도)
        rectTransform.localScale = initialScale * scaleMultiplier;
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false; // 드래그 중 마우스가 아이콘을 통과해 바닥을 인식하게 함
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 4. 마우스 위치로 아이콘 이동
        rectTransform.position = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 5. 원래 부모와 순서로 복귀
        transform.SetParent(originalParent);
        transform.SetSiblingIndex(originalIndex);

        // 6. 상태 원상 복구
        rectTransform.localScale = initialScale;
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;

        // 7. 슬롯 중앙으로 위치 초기화
        rectTransform.anchoredPosition = Vector2.zero;
    }
}