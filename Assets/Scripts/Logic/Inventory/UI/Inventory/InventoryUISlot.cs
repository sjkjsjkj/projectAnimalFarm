using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 인벤토리UI 의 각각의 슬롯UI 입니다.
/// </summary>
//[RequireComponent(typeof(Button))]
public class InventoryUISlot : BaseMono, IBeginDragHandler, IDragHandler ,IEndDragHandler, IDropHandler
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] protected Image _itemIcon;
    [SerializeField] protected Image _itemStackCountImg;          //아이템 슬롯에 중첩된 갯수를 이미지로 표현할 때 사용
    [SerializeField] protected TextMeshProUGUI _itemStackCountTxt; //아이템 슬롯에 중첩된 갯수를 텍스트로 표현할 때 사용
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    protected InventoryUI _inventoryUI;
    protected int _thisSlotIdx;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int Index => _thisSlotIdx;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void SetInfo(InventoryUI invenUI, int slotIdx)
    {
        SlotClear();
        _inventoryUI = invenUI;
        _thisSlotIdx = slotIdx;
    }
    public void SetInfo(Sprite itemIcon, int itemStackCount)
    {
        if(itemIcon == null)
        {
            UDebug.Print("아이템 스프라이트가 없음");
            return;
        }
        _itemIcon.sprite = itemIcon;
        _itemStackCountTxt.text = itemStackCount.ToString();
    }
    public void SlotClear()
    {
        _itemIcon.sprite = null;
        _itemStackCountTxt.text = "";
    }
    public void UseSlot()
    {

    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        //eventData.useDragThreshold = false;
        _inventoryUI.BeginDrag(_thisSlotIdx);
    }
    public void OnDrag(PointerEventData eventData)
    {
        _inventoryUI.DragIng();
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        _inventoryUI.EndDrag();
    }
    public void OnDrop(PointerEventData eventData)
    {
        

        _inventoryUI.DragDrop(_thisSlotIdx);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
