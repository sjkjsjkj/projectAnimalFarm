using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class CraftRecipeBtn : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("주제")]
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _recipeItemName;
    #endregion

    public event Action<int> OnRecipeButtonClick;

    private WorkbenchUI _workbenchUI;
    private int _recipeSlotIdx;
    public void SetInfo(Sprite icon, string name, WorkbenchUI workbenchUI, int idx)
    {
        _image.sprite = icon;
        _recipeItemName.text = name;
        _workbenchUI = workbenchUI;
        _recipeSlotIdx = idx;
    }

    public void CraftButtonClick()
    {
        OnRecipeButtonClick?.Invoke(_recipeSlotIdx);
    }
}
