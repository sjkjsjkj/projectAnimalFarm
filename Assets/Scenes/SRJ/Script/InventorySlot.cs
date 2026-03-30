using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    [Header("UI References")]
    public Image iconImage;
    public GameObject itemObject;
    public GameObject contextMenuPrefab;
    public TextMeshProUGUI countText;

    private ItemData currentItem;
    private int itemCount = 0;
    private Canvas rootCanvas;

    public bool IsEmpty => currentItem == null;
    public ItemData CurrentItem => currentItem;
    public int ItemCount => itemCount;

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (countText != null) countText.text = "";
    }

    public void SetItem(ItemData item, int count = 1)
    {
        currentItem = item;
        itemCount = count;
        iconImage.sprite = item.icon;
        iconImage.color = Color.white;
        itemObject.SetActive(true);
        UpdateCountText();

        var draggable = itemObject.GetComponent<DraggableItem>();
        if (draggable != null) draggable.Init(this);
    }

    public void AddCount(int amount)
    {
        itemCount += amount;
        if (itemCount > currentItem.maxStack)
            itemCount = currentItem.maxStack;
        UpdateCountText();
    }

    void UpdateCountText()
    {
        if (countText == null) return;
        countText.text = itemCount > 1 ? itemCount.ToString() : "";
    }

    public void ClearSlot()
    {
        currentItem = null;
        itemCount = 0;
        iconImage.sprite = null;
        iconImage.color = Color.clear;
        itemObject.SetActive(false);
        if (countText != null) countText.text = "";
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null) return;

        InventorySlot fromSlot = dragged.OriginalSlot;
        if (fromSlot == null || fromSlot == this || fromSlot.IsEmpty) return;

        ItemData fromItem = fromSlot.CurrentItem;
        int fromCount = fromSlot.ItemCount;
        ItemData toItem = currentItem;
        int toCount = itemCount;

        fromSlot.ClearSlot();
        if (toItem != null) fromSlot.SetItem(toItem, toCount);
        ClearSlot();
        SetItem(fromItem, fromCount);

        // 드래그 오브젝트 위치 정리
        dragged.transform.SetParent(fromSlot.transform, false);
        dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && !IsEmpty)
        {
            var existing = FindObjectOfType<InventoryContextMenu>();
            if (existing != null) Destroy(existing.gameObject);

            GameObject menuGO = Instantiate(contextMenuPrefab, rootCanvas.transform);
            InventoryContextMenu menu = menuGO.GetComponent<InventoryContextMenu>();
            menu.Open(this, eventData.position);
        }
    }
}
