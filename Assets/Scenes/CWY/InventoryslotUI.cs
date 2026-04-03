using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 한 칸 UI
/// 아이콘 / 이름 / 수량을 표시한다.
/// </summary>
public class InventorySlotUI : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _amountText;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public void SetData(SheetItemRow row, int amount, Sprite icon)
    {
        if (row == null)
        {
            Clear();
            return;
        }

        if (_iconImage != null)
        {
            _iconImage.sprite = icon;
            _iconImage.enabled = icon != null;
        }

        if (_nameText != null)
        {
            _nameText.text = row.name;
        }

        if (_amountText != null)
        {
            _amountText.text = amount.ToString();
        }
    }

    public void Clear()
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = null;
            _iconImage.enabled = false;
        }

        if (_nameText != null)
        {
            _nameText.text = string.Empty;
        }

        if (_amountText != null)
        {
            _amountText.text = string.Empty;
        }
    }
    #endregion
}
