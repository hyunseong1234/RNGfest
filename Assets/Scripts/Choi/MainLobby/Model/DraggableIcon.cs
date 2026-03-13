using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableIcon : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 initialScale;

    public float scaleMultiplier = 1.2f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        initialScale = rectTransform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        originalParent = transform.parent;
        // 드래그 레이어 혹은 최상단 Canvas로 이동하여 다른 UI에 가려지지 않게 함
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        rectTransform.localScale = initialScale * scaleMultiplier;
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData) => rectTransform.position = eventData.position;

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        rectTransform.localScale = initialScale;
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;
        rectTransform.anchoredPosition = Vector2.zero; // 원래 슬롯 중앙으로 복귀
    }
}