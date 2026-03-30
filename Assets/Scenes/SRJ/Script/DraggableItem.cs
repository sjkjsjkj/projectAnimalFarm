using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private InventorySlot originalSlot;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private bool isDragging = false;

    public InventorySlot OriginalSlot => originalSlot;
    public ItemData ItemData => originalSlot?.CurrentItem;

    public void Init(InventorySlot slot)
    {
        originalSlot = slot;
        rootCanvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 빈 슬롯 드래그 차단
        if (originalSlot == null || originalSlot.IsEmpty)
        {
            eventData.pointerDrag = null;
            return;
        }

        isDragging = true;
        originalParent = transform.parent;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        // 드롭 실패 시 원래 슬롯으로 복귀
        if (transform.parent == rootCanvas.transform)
        {
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        originalParent = null;
    }
}
