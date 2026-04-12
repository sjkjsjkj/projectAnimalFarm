using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class CraftCategoryBtn : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("주제")]
    [SerializeField] private ECraftableItemType _type;
    [SerializeField] private WorkbenchUI _workbenchUI;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void CategoryBtnClick()
    {
        _workbenchUI.CategoryBtnClick(_type);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
