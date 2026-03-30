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
    private Vector2 originalPosition;

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
        if (originalSlot == null || originalSlot.IsEmpty) return;

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 드롭 실패 시 (빈 곳에 드롭) 원래 슬롯으로 복귀
        if (transform.parent == rootCanvas.transform)
        {
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}
