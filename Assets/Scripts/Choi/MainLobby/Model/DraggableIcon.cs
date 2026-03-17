using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableIcon : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 initialScale;
    private Vector2 startPos; // [추가] 시작 지점 체크용
    private bool isDragging = false; // [추가] 드래그 중인지 판별

    public float scaleMultiplier = 1.2f;
    public float dragThreshold = 5.0f; // [추가] 이 거리만큼 움직여야 드래그로 인정

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        initialScale = rectTransform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startPos = eventData.position;
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 1. 일정 거리 이상 움직였을 때만 드래그 시작 처리
        if (!isDragging && Vector2.Distance(startPos, eventData.position) > dragThreshold)
        {
            isDragging = true;
            originalParent = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();

            rectTransform.localScale = initialScale * scaleMultiplier;
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
        }

        // 2. 드래그 중일 때만 위치 이동
        if (isDragging)
        {
            rectTransform.position = eventData.position;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging)
        {
            transform.SetParent(originalParent);
            rectTransform.localScale = initialScale;
            canvasGroup.alpha = 1.0f;
            canvasGroup.blocksRaycasts = true;
            rectTransform.anchoredPosition = Vector2.zero;
        }
        isDragging = false;
    }

    // 3. [핵심] 드래그를 안 했을 때만 클릭 이벤트 발생!
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging)
        {
            // SelectFramImage의 정보창 열기 함수 호출
            GetComponent<SelectFramImage>().OpenInfo();
        }
    }
}