using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제작대 클래스 입니다.
/// </summary>
public class Workbench :  ICraftLogical
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    //작업대 전체를 위한 내부변수
    private RecipeTableSO _recipeTables;

    //각각의 작업을 할 때 사용할 내부변수
    private RecipeSO _recipe;
    private RequireItemSO[] _requiredItems;
    private WorkbenchReturnStruct[] _curHasItemConditions;


    private List<RecipeSO> _axeRecipies;
    private List<RecipeSO> _fishigRodRecipies;
    private List<RecipeSO> _shovelRecipies;
    private List<RecipeSO> _pickaxeRecipies;
    private List<RecipeSO> _wateringCanRecipe;
    private List<RecipeSO> _sickleRecipies;

    private bool _isInitialized = false;
    private bool _canMake;
    private Inventory _playerInventory;
    #endregion

    public Workbench()
    {
        Initialize();
        SetAllRecipe();
    }

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public event Action<WorkbenchReturnStruct[],bool> OnChenageRecipe;

    #region 인터페이스 약속
   
    public WorkbenchReturnStruct[] GetMaterials(string recipeId)
    {

        throw new NotImplementedException();
        //RecipeSO tempRecipe = DatabaseManager.In
        //return FindItem()
    }

    public bool TryCraftItem(string recipeId, out string message)
    {
        throw new NotImplementedException();
    }
    #endregion

    public void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _recipeTables = Resources.Load<RecipeTableSO>("Table/RecipeTableSO");
        _playerInventory = InventoryManager.Ins.PlayerInventory;

        _axeRecipies = new List<RecipeSO>();
        _fishigRodRecipies = new List<RecipeSO>();
        _shovelRecipies = new List<RecipeSO>();
        _pickaxeRecipies = new List<RecipeSO>();
        _wateringCanRecipe = new List<RecipeSO>();
        _sickleRecipies = new List<RecipeSO>();

        

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }

    //레시피를 선택했을 때 불러와질 메서드.
    //    public void SetRecipe(RecipeSO recipe)
    public void SetRecipe(ECraftableItemType curCategory, int recipeIdx)
    {
        
        _recipe = GetCurCategoryRecipe(curCategory, recipeIdx);
        _requiredItems = _recipe.RequiedItems;
        _canMake = true;

        _curHasItemConditions = FindItem(_requiredItems);

        OnChenageRecipe?.Invoke(_curHasItemConditions, _canMake);
    }
    
   
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
        //if (_playerInventory.TryGetItem(tempItemSO))
        if (ItemCollectionCoordinator.Ins.TryCollectItem(tempItemSO))
        {
            //성공
            //TODO : 인벤토리의 아이템 제거
            for (int i = 0; i < _requiredItems.Length; i++)
            {
                //UDebug.Print($"현재 필요한 아이템 : {_requiredItems[i].RequireItem.Name}");
                _playerInventory.TryRemoveItem(_requiredItems[i].RequireItem.Id, _requiredItems[i].Count);
            }
        }
        else
        {
            //실패
            //TODO : UI를 띄우고 호출 스택 초기화
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    //부팅 시 모든 레시피의 정보를 담아올 메서드
    private void SetAllRecipe()
    {
        
        if(DataManager.Ins == null)
        {
            UDebug.Print("데이터 오류",UnityEngine.LogType.Assert);
            return;
        }

        //_recipeTables = 

        List<RecipeSO> tempRecipeSOList = _recipeTables.ElementalList;

        for (int i=0; i< tempRecipeSOList.Count; i++)
        {
            switch(tempRecipeSOList[i].TargetItemType)
            {
                case EType.AxeItem:
                    _axeRecipies.Add(tempRecipeSOList[i]);
                    break;
                case EType.WateringCan:
                    _wateringCanRecipe.Add(tempRecipeSOList[i]);
                    break;
                case EType.ShovelItem:
                    _shovelRecipies.Add(tempRecipeSOList[i]);
                    break;
                case EType.PickaxeItem:
                    _pickaxeRecipies.Add(tempRecipeSOList[i]);
                    break;
                case EType.Fishingrod:
                    _fishigRodRecipies.Add(tempRecipeSOList[i]);
                    break;
                case EType.SickleItem:
                    _sickleRecipies.Add(tempRecipeSOList[i]);
                    break;
            }
        }
    }

    //현재 UI에서 선택한 카테고리와 Idx로 아이템 레시피 반환하는 메서드
    private RecipeSO GetCurCategoryRecipe(ECraftableItemType curCategory, int recipeIdx)
    {
        switch (curCategory)
        {
            case ECraftableItemType.Axe:
                return _axeRecipies[recipeIdx];
            case ECraftableItemType.Sickle:
                return _sickleRecipies[recipeIdx];
            case ECraftableItemType.Shovel:
                return _shovelRecipies[recipeIdx];
            case ECraftableItemType.PickAxe:
                return _pickaxeRecipies[recipeIdx];
            case ECraftableItemType.WateringCan:
                return _wateringCanRecipe[recipeIdx];
            case ECraftableItemType.FishingRod:
                return _fishigRodRecipies[recipeIdx];
            default:
                return null;
        }

    }
    /// <summary>
    /// UI 에서 카테고리를 셀렉트 했을 때 
    /// </summary>
    /// <returns></returns>
    public List<RecipeSO> SelectCategory(ECraftableItemType eType)
    {
       // UDebug.Print($"{eType.ToString()} | {_axeRecipies.Count}");
        switch (eType)
        {
            case ECraftableItemType.Axe:
                return _axeRecipies;
            case ECraftableItemType.Sickle:
                return _sickleRecipies;
            case ECraftableItemType.Shovel:
                return _shovelRecipies;
            case ECraftableItemType.PickAxe:
                return _pickaxeRecipies;
            case ECraftableItemType.WateringCan:
                return _wateringCanRecipe;
            case ECraftableItemType.FishingRod:
                return _fishigRodRecipies;
            default:
                return null;
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

    //인벤토리에서 아이템을 찾아 현재 레시피와 비교하여 전달해야하는 목록들을 구조체로 만들어서 반환함.
    private WorkbenchReturnStruct[] FindItem(RequireItemSO[] requireItems)
    {
        WorkbenchReturnStruct[] tempStructs = new WorkbenchReturnStruct[requireItems.Length];
        for (int i = 0; i < _requiredItems.Length; i++)
        {
            int tempCurCount = _playerInventory.FindItemToInventory(requireItems[i].RequireItem.Id);
            int tempReqCount = requireItems[i].Count;

            //테스트용
            tempStructs[i] = new WorkbenchReturnStruct(requireItems[i].RequireItem.Image, requireItems[i].RequireItem.Name, tempCurCount, tempReqCount, i, tempCurCount >= tempReqCount ? true : false);
            _canMake = tempStructs[i].IsCondition ? _canMake : false;
            //tempStructs[i] = new WorkbenchReturnStruct(tempCurCount, tempReqCount, tempCurCount >= tempReqCount ? true : false);
        }
        return tempStructs;
    }
    #endregion
}
