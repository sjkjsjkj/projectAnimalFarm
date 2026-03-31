using System;

/// <summary>
/// 제작대 클래스 입니다.
/// </summary>
public class Workbench : Singleton<Workbench>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private RecipeSO _recipe;
    private RequireItemSO[] _requiredItems;
    private WorkbenchReturnStruct[] _curHasItemConditions;

    private bool _isInitialized = false;

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public event Action<WorkbenchReturnStruct[]> OnChenageRecipe;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void SetRecipe(RecipeSO recipe)
    {
        UDebug.Print("Set Recipe");
        _recipe = recipe;
        _requiredItems = recipe.RequiedItems;
        _curHasItemConditions = FindItem(_requiredItems);

        OnChenageRecipe?.Invoke(_curHasItemConditions);
    }
    private WorkbenchReturnStruct[] FindItem(RequireItemSO[] requireItems)
    {
        WorkbenchReturnStruct[] tempStructs = new WorkbenchReturnStruct[requireItems.Length];
        for (int i = 0; i < _requiredItems.Length; i++)
        {
            int tempCurCount = InventoryManager.Ins.PlayerInventory.FindItem(requireItems[i].Id);
            int tempReqCount = requireItems[i].Count;
            tempStructs[i] = new WorkbenchReturnStruct(tempCurCount, tempReqCount, tempCurCount >= tempReqCount ? true : false);
        }
        return tempStructs;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion
}


