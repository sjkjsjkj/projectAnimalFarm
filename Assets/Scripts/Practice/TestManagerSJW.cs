using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 싱글톤 클래스의 설계 의도입니다.
/// </summary>
public class TestManagerSJW : Singleton<TestManagerSJW>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("테스트 ID")]
    [SerializeField] private int _testTileID;

    [Header("동물 테스트")]
    [SerializeField] private string _animalID;
    [SerializeField] private BreedingArea _breedingArea;

    [Header("농장 테스트")]
    [SerializeField] private FarmArea _farmArea;
    [SerializeField] private int _pos;
    [SerializeField] private EHarvest _seedId;
    [SerializeField] private string _seedItemId;

    [Header("인벤토리 테스트")]
    [SerializeField] private string _itemId;
    [SerializeField] private ItemCollectionCoordinator _itemCoordinatior;
    

    [Header("제작대 테스트")]
    [SerializeField] private RecipeSO[] _recipeSOs;
    [SerializeField] private int _recipeIdx;
    [SerializeField] private Button _makeButton;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    //private FarmArea _testFarmArea;

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }

        InitSetting();

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    private void InitSetting()
    {
    }
    [ContextMenu("AnimalSpawnTest")]
    public void TestAnimalSpawn()
    {
        _breedingArea.SpawnAnimal(_animalID);
    }
    public void TestAllAnimalSpawn()
    {
        _breedingArea.TestFunction();
    }
    public void TestGetItemInvenDir()
    {
        ItemSO tempItemSO = DatabaseManager.Ins.Item(_itemId);

        InventoryManager.Ins.PlayerInventory.TryGetItem(tempItemSO);
    }
    public void TestGetItemInvenCoordinator()
    {
        ItemCollectionCoordinator.Ins.TryCollectItem(Id.stone_with_minerals_26, 1);
        ItemCollectionCoordinator.Ins.TryCollectItem(Id.stone_with_minerals_25, 1);
        ItemCollectionCoordinator.Ins.TryCollectItem(Id.stone_with_minerals_24, 1);
        ItemCollectionCoordinator.Ins.TryCollectItem(Id.stone_with_minerals_22, 1);
        ItemCollectionCoordinator.Ins.TryCollectItem(Id.stone_with_minerals_27, 1);
        ItemCollectionCoordinator.Ins.TryCollectItem(Id.stone_with_minerals_28, 1);
        ItemCollectionCoordinator.Ins.TryCollectItem(Id.stone_with_minerals_29, 1);
        ItemCollectionCoordinator.Ins.TryCollectItem(Id.stone_with_minerals_30, 1);
        ItemCollectionCoordinator.Ins.TryCollectItem(Id.stone_with_minerals_31, 1);
        ItemCollectionCoordinator.Ins.TryCollectItem(Id.stone_with_minerals_45, 1);
    }

    public void TestFarm()
    {
        _farmArea.TestFunction(_pos, _seedId.ToString());
    }

    public void TestCheckRecipe()
    {
        UDebug.Print("제작대 테스트");

        WorkbenchManager.Ins.Workbench.OnChenageRecipe -= ShowCurrentRequireCondition;
        WorkbenchManager.Ins.Workbench.OnChenageRecipe += ShowCurrentRequireCondition;

        //Workbench.Ins.SetRecipe(_recipeSOs[_recipeIdx]);
    }

    public void ShowCurrentRequireCondition(WorkbenchReturnStruct[] curRequireCondition, bool canMake)
    {
        for (int i = 0; i < curRequireCondition.Length; i++)
        {
            UDebug.Print($"요구 아이템 {i} [{_recipeSOs[_recipeIdx].RequiedItems[i].RequireItem.Id}] : {curRequireCondition[i].CurHasCount} / {curRequireCondition[i].RequireCount} | 제작 가능 여부 : {curRequireCondition[i].IsCondition}");
        }
        if(canMake)
        {
            _makeButton.gameObject.SetActive(true);
        }
        else
        {
            _makeButton.gameObject.SetActive(false);
        }
    }
    public void TestMakeItem()
    {
        WorkbenchManager.Ins.Workbench.MakeItem();

        //Workbench.Ins.SetRecipe(_recipeSOs[_recipeIdx]);
    }
    public void FlagTest()
    {
        uint flag1 = 0, flag2 = 0, flag3 = 0;
        flag1 |= (uint)(EConnectionDir.Left | EConnectionDir.Right);
        flag2 |= (uint)(EConnectionDir.Left | EConnectionDir.Right | EConnectionDir.Up);
        flag3 |= (uint)EConnectionDir.Left;
        UDebug.Print($"Flag 1 : LeftRight {flag1} | Flag 2 : LeftRightUp {flag2} | Flag 3 : None {flag3} ");
        UDebug.Print($"Flag 1 & Flag 3 : { flag1 ^ flag3} ");
    }
    public void TestGetMoney()
    {
        DataManager.Ins.Player.AddMoney(10000);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Start()
    {
 
    }
    #endregion
}
