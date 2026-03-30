using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 인벤토리UI 의 각각의 슬롯UI 입니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class InventoryUISlot : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] protected Image _itemIcon;
    [SerializeField] protected Image _itemStackCountImg;          //아이템 슬롯에 중첩된 갯수를 이미지로 표현할 때 사용
    [SerializeField] protected TextMeshProUGUI _itemStackCountTxt; //아이템 슬롯에 중첩된 갯수를 텍스트로 표현할 때 사용
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void SetInfo()
    {
        _itemIcon = null;
        _itemStackCountTxt.text = "";
    }
    public void SetInfo(Sprite itemIcon, int itemStackCount)
    {
        _itemIcon.sprite = itemIcon;
        _itemStackCountTxt.text = itemStackCount.ToString();
    }
    public void SlotClear()
    {
        SetInfo();
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
