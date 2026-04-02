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
    private bool _canMake;
    private Inventory _playerInventory;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public event Action<WorkbenchReturnStruct[],bool> OnChenageRecipe;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void SetRecipe(RecipeSO recipe)
    {
        UDebug.Print("Set Recipe");
        _recipe = recipe;
        _requiredItems = recipe.RequiedItems;
        _canMake = true;

        _curHasItemConditions = FindItem(_requiredItems);

        OnChenageRecipe?.Invoke(_curHasItemConditions, _canMake);
    }
    private WorkbenchReturnStruct[] FindItem(RequireItemSO[] requireItems)
    {
        
        WorkbenchReturnStruct[] tempStructs = new WorkbenchReturnStruct[requireItems.Length];
        for (int i = 0; i < _requiredItems.Length; i++)
        {
            int tempCurCount = _playerInventory.FindItemToInventory(requireItems[i].Id);
            int tempReqCount = requireItems[i].Count;

            //테스트용
            tempStructs[i] = new WorkbenchReturnStruct(tempCurCount, tempReqCount, i, tempCurCount >= tempReqCount ? true : false);
            _canMake = tempStructs[i].IsCondition ? _canMake : false;
            //tempStructs[i] = new WorkbenchReturnStruct(tempCurCount, tempReqCount, tempCurCount >= tempReqCount ? true : false);
        }
        return tempStructs;
    }
    #endregion
    /// <summary>
    /// 제작대 UI에서 제작버튼을 눌렀을 때 실행되는 메서드
    /// </summary>
    public void MakeItem()
    {
        if (_requiredItems == null || _requiredItems.Length == 0)
        {
            //레시피가 선택되지 않으면 제작버튼이 비활성화 되기 때문에 여기가 체크될 일은 없음.
            UDebug.Print("레시피가 선택되지 않음.");
            return;
        }
        //타겟의 ID로 ItemSO를 반환받음.
        ItemSO tempItemSO = ReturnItemSO(_recipe.TargetItemType, _recipe.TargetItemId);

        //현재 플레이어의 인벤토리에 아이템을 넣을 수 있는 상황인지 (아이템창이 꽉 찼거나 하는 등의 이유 체크)
        //성공하면 자동으로 인벤에 아이템 추가 됨.
        if (_playerInventory.TryGetItem(tempItemSO))
        {
            //성공
            //TODO : 인벤토리의 아이템 제거
            for(int i=0; i<_requiredItems.Length; i++)
            {
                UDebug.Print($"현재 필요한 아이템 : {_requiredItems[i].Id}");
                _playerInventory.TryRemoveItem(_requiredItems[i].Id, _requiredItems[i].Count);
            }
        }
        else
        {
            //실패
            //TODO : UI를 띄우고 호출 스택 초기화
        }
    }



    //타겟의 ID를 받아 데이터베이스에서 ItemSO로 반환해주는 메서드
    private ItemSO ReturnItemSO(EType targetItemType, string targetItemID)
    {
        ItemSO tempItemSO;
        switch (targetItemType)
        {
            case EType.AxeItem:
            case EType.SickleItem:
            case EType.ShovelItem:
            case EType.PickaxeItem:
            case EType.WateringCan:
            case EType.Fishingrod:
                tempItemSO = DatabaseManager.Ins.ToolItem(targetItemID);
                break;
            case EType.SeedItem:
                tempItemSO = DatabaseManager.Ins.SeedItem(targetItemID);
                break;
            //Todo : 다른 아이템이 들어온다면..
            default:
                tempItemSO = null;
                break;
        }
        return tempItemSO;
    }
    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }
        _playerInventory = InventoryManager.Ins.PlayerInventory;
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion
}


