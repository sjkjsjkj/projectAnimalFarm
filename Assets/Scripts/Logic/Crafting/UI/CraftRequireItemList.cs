using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class CraftRequireItemList : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("주제")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _curItemNameTxt;
    [SerializeField] private TextMeshProUGUI _curItemCountTxt;
    [SerializeField] private TextMeshProUGUI _reqItemCountTxt;
    #endregion

    public void SetInfo(WorkbenchReturnStruct context)
    {
        _itemIcon.sprite = context.Icon;
        _curItemNameTxt.text = context.ItemName;
        _curItemCountTxt.text = context.CurHasCount.ToString();
        _reqItemCountTxt.text = context.RequireCount.ToString();
        if(context.IsCondition)
        {
            _curItemCountTxt.color = Color.black;
        }
        else
        {
            _curItemCountTxt.color = Color.red;
        }
    }
}
