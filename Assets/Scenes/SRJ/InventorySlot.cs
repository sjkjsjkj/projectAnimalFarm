using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IDropHandler,
    IPointerEnterHandler, IPointerExitHandler  // 툴팁용 추가
{
    [Header("UI References")]
    public Image iconImage;
    public GameObject itemObject;
    public GameObject contextMenuPrefab;
    public TextMeshProUGUI countText; // 추가

    private ItemData currentItem;
    private int itemCount = 0; // 추가
    private Canvas rootCanvas;

    public bool IsEmpty => currentItem == null;
    public ItemData CurrentItem => currentItem;
    public int ItemCount => itemCount;

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
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
        // 1개면 숫자 숨기기, 2개 이상이면 표시
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

    // 툴팁 - 마우스 올릴 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsEmpty)
            TooltipUI.Instance.Show(currentItem);
    }

    // 툴팁 - 마우스 나갈 때
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null) return;

        InventorySlot fromSlot = dragged.OriginalSlot;
        if (fromSlot == this) return;

        ItemData fromItem = fromSlot.CurrentItem;
        int fromCount = fromSlot.ItemCount;
        ItemData toItem = currentItem;
        int toCount = itemCount;

        fromSlot.ClearSlot();
        if (toItem != null) fromSlot.SetItem(toItem, toCount);

        ClearSlot();
        SetItem(fromItem, fromCount);

        dragged.transform.SetParent(fromSlot.transform.Find("Icon").transform.parent, false);
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
