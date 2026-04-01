//using System;
//using TMPro.EditorUtilities;

///// <summary>
///// 제작대 클래스 입니다.
///// </summary>
//public class Workbench
//{
//    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
//    private RecipeSO _recipe;
//    private RequireItemSO[] _requiredItems;
//    private WorkbenchReturnStruct[] _curHasItemConditions;
//    #endregion

//    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
//    public event Action<WorkbenchReturnStruct[]> OnChenageRecipe;
//    #endregion

//    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
//    public void SetRecipe(RecipeSO recipe)
//    {

//        _recipe = recipe;
//        _requiredItems = recipe.RequiedItems;
//        _curHasItemConditions = FindItem(_requiredItems);

//        OnChenageRecipe?.Invoke(_curHasItemConditions);
//    }
//    private WorkbenchReturnStruct[] FindItem(RequireItemSO[] requireItems)
//    {
//        WorkbenchReturnStruct[] tempStructs = new WorkbenchReturnStruct[requireItems.Length];
//        for (int i = 0; i < _requiredItems.Length; i++)
//        {
//            int tempCount = InventoryManager.Ins.PlayerInventory.FindItem(requireItems[i].Id);
//            tempStructs[i] = new WorkbenchReturnStruct(tempCount, tempCount >= requireItems[i].Count ? true : false);
//        }
//        return tempStructs;
//    }
//    #endregiona
//}
