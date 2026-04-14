using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 테스트 전용 매니저
/// 
/// 이번 수정 내용
/// 1. itemId / amount를 인스펙터에서 넣고 바로 인벤토리에 추가 가능
/// 2. ContextMenu로 클릭 한 번에 테스트 가능
/// 3. 기본 도구 세트 / 상위 도구 세트도 바로 지급 가능
/// 4. ItemCollectionCoordinator 경유 방식으로 팀 로직 흐름 그대로 테스트 가능
/// </summary>
public class TestManagerCWY : Singleton<TestManagerCWY>
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
    [SerializeField] private string _itemId = "Item_Tool_BasicWateringCan";
    [SerializeField] private int _itemAmount = 1;
    [SerializeField] private bool _useCoordinatorForInventoryTest = true;
    [SerializeField] private ItemCollectionCoordinator _itemCoordinatior;

    [Header("제작대 테스트")]
    [SerializeField] private RecipeSO[] _recipeSOs;
    [SerializeField] private int _recipeIdx;
    [SerializeField] private Button _makeButton;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 초기화 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        InitSetting();
        _isInitialized = true;
    }

    private void InitSetting()
    {
        if (_itemCoordinatior == null)
        {
            _itemCoordinatior = ItemCollectionCoordinator.Ins;
        }
    }
    #endregion

    #region ─────────────────────────▶ 공용 테스트 유틸 ◀─────────────────────────
    private bool TryGiveItem(string itemId, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            UDebug.Print("[TestManagerSJW] itemId가 비어 있습니다.", LogType.Warning);
            return false;
        }

        if (amount <= 0)
        {
            UDebug.Print($"[TestManagerSJW] amount가 0 이하입니다. itemId={itemId}, amount={amount}", LogType.Warning);
            return false;
        }

        if (DatabaseManager.Ins == null)
        {
            UDebug.Print("[TestManagerSJW] DatabaseManager.Ins가 null입니다.", LogType.Assert);
            return false;
        }

        ItemSO tempItemSO = DatabaseManager.Ins.Item(itemId);
        if (tempItemSO == null)
        {
            UDebug.Print($"[TestManagerSJW] DatabaseManager에서 itemId에 해당하는 ItemSO를 찾지 못했습니다. itemId={itemId}", LogType.Warning);
            return false;
        }

        if (_useCoordinatorForInventoryTest)
        {
            if (_itemCoordinatior == null)
            {
                _itemCoordinatior = ItemCollectionCoordinator.Ins;
            }

            if (_itemCoordinatior == null)
            {
                UDebug.Print("[TestManagerSJW] ItemCollectionCoordinator가 없습니다.", LogType.Assert);
                return false;
            }

            bool result = _itemCoordinatior.TryCollectItem(itemId, amount);

            if (result)
            {
                UDebug.Print($"[TestManagerSJW] Coordinator 경유 아이템 지급 성공: {itemId} x{amount}");
            }
            else
            {
                UDebug.Print($"[TestManagerSJW] Coordinator 경유 아이템 지급 실패: {itemId} x{amount}", LogType.Warning);
            }

            return result;
        }

        if (InventoryManager.Ins == null)
        {
            UDebug.Print("[TestManagerSJW] InventoryManager.Ins가 null입니다.", LogType.Assert);
            return false;
        }

        if (InventoryManager.Ins.PlayerInventory == null)
        {
            UDebug.Print("[TestManagerSJW] PlayerInventory가 null입니다.", LogType.Assert);
            return false;
        }

        bool isAllSuccess = true;

        for (int i = 0; i < amount; i++)
        {
            bool result = InventoryManager.Ins.PlayerInventory.TryGetItem(tempItemSO);
            if (!result)
            {
                isAllSuccess = false;
                UDebug.Print($"[TestManagerSJW] 직접 인벤토리 추가 실패: {itemId}, count={i + 1}/{amount}", LogType.Warning);
                break;
            }
        }

        if (isAllSuccess)
        {
            UDebug.Print($"[TestManagerSJW] 직접 인벤토리 추가 성공: {itemId} x{amount}");
        }

        return isAllSuccess;
    }

    private void GiveToolSet(
        string wateringCanId,
        string shovelId,
        string pickAxeId,
        string sickleId,
        string fishingRodId)
    {
        TryGiveItem(wateringCanId, 1);
        TryGiveItem(shovelId, 1);
        TryGiveItem(pickAxeId, 1);
        TryGiveItem(sickleId, 1);
        TryGiveItem(fishingRodId, 1);
    }
    #endregion

    #region ─────────────────────────▶ 테스트 메서드 ◀─────────────────────────
    [ContextMenu("AnimalSpawnTest")]
    public void TestAnimalSpawn()
    {
        _breedingArea.SpawnAnimal(_animalID);
    }

    public void TestAllAnimalSpawn()
    {
        _breedingArea.TestFunction();
    }

    [ContextMenu("Inventory/Give Item By Inspector Id")]
    public void TestGiveItemByInspectorId()
    {
        TryGiveItem(_itemId, _itemAmount);
    }

    [ContextMenu("Inventory/Give Basic Tool Set")]
    public void TestGiveBasicToolSet()
    {
        GiveToolSet(
            "Item_Tool_BasicWateringCan",
            "Item_Tool_BasicShovel",
            "Item_Tool_BasicPickAxe",
            "Item_Tool_BasicSickle",
            "Item_Tool_BasicFishingRod");
    }

    [ContextMenu("Inventory/Give Solid Tool Set")]
    public void TestGiveSolidToolSet()
    {
        GiveToolSet(
            "Item_Tool_SolidWateringCan",
            "Item_Tool_SolidShovel",
            "Item_Tool_SolidPickAxe",
            "Item_Tool_SolidSickle",
            "Item_Tool_SolidFishingRod");
    }

    [ContextMenu("Inventory/Give Super Tool Set")]
    public void TestGiveSuperToolSet()
    {
        GiveToolSet(
            "Item_Tool_SuperWateringCan",
            "Item_Tool_SuperShovel",
            "Item_Tool_SuperPickAxe",
            "Item_Tool_SuperSickle",
            "Item_Tool_SuperFishingRod");
    }

    [ContextMenu("Inventory/Give Prime Tool Set")]
    public void TestGivePrimeToolSet()
    {
        GiveToolSet(
            "Item_Tool_PrimeWateringCan",
            "Item_Tool_PrimeShovel",
            "Item_Tool_PrimePickAxe",
            "Item_Tool_PrimeSickle",
            "Item_Tool_PrimeFishingRod");
    }

    [ContextMenu("Inventory/Give Master Tool Set")]
    public void TestGiveMasterToolSet()
    {
        GiveToolSet(
            "Item_Tool_MasterWateringCan",
            "Item_Tool_MasterShovel",
            "Item_Tool_MasterPickAxe",
            "Item_Tool_MasterSickle",
            "Item_Tool_MasterFishingRod");
    }

    // 기존 메서드 유지
    public void TestGetItemInvenDir()
    {
        ItemSO tempItemSO = DatabaseManager.Ins.Item(_itemId);

        if (tempItemSO == null)
        {
            UDebug.Print($"[TestManagerSJW] tempItemSO가 null입니다. itemId={_itemId}", LogType.Warning);
            return;
        }

        if (InventoryManager.Ins == null || InventoryManager.Ins.PlayerInventory == null)
        {
            UDebug.Print("[TestManagerSJW] PlayerInventory를 찾지 못했습니다.", LogType.Assert);
            return;
        }

        InventoryManager.Ins.PlayerInventory.TryGetItem(tempItemSO);
    }

    public void TestGetItemInvenCoordinator()
    {
        if (_itemCoordinatior == null)
        {
            _itemCoordinatior = ItemCollectionCoordinator.Ins;
        }

        if (_itemCoordinatior == null)
        {
            UDebug.Print("[TestManagerSJW] ItemCollectionCoordinator가 없습니다.", LogType.Assert);
            return;
        }

        _itemCoordinatior.TryCollectItem("stone with minerals_26", 1);
        _itemCoordinatior.TryCollectItem("stone with minerals_25", 1);
        _itemCoordinatior.TryCollectItem("stone with minerals_24", 1);
        _itemCoordinatior.TryCollectItem("stone with minerals_22", 1);
        _itemCoordinatior.TryCollectItem("stone with minerals_27", 1);
        _itemCoordinatior.TryCollectItem("stone with minerals_28", 1);
        _itemCoordinatior.TryCollectItem("stone with minerals_29", 1);
        _itemCoordinatior.TryCollectItem("stone with minerals_30", 1);
        _itemCoordinatior.TryCollectItem("stone with minerals_31", 1);
        _itemCoordinatior.TryCollectItem("stone with minerals_45", 1);
    }

    public void TestFarm()
    {
        _farmArea.TestFunction(_pos, _seedId.ToString());
    }

    public void TestCheckRecipe()
    {
        UDebug.Print("제작대 테스트");
    }

    public void ShowCurrentRequireCondition(WorkbenchReturnStruct[] curRequireCondition, bool canMake)
    {
        for (int i = 0; i < curRequireCondition.Length; i++)
        {
            UDebug.Print($"요구 아이템 {i} [{_recipeSOs[_recipeIdx].RequiedItems[i].RequireItem.Id}] : {curRequireCondition[i].CurHasCount} / {curRequireCondition[i].RequireCount} | 제작 가능 여부 : {curRequireCondition[i].IsCondition}");
        }

        if (canMake)
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
    }

    public void FlagTest()
    {
        uint flag1 = 0, flag2 = 0, flag3 = 0;
        flag1 |= (uint)(EConnectionDir.Left | EConnectionDir.Right);
        flag2 |= (uint)(EConnectionDir.Left | EConnectionDir.Right | EConnectionDir.Up);
        flag3 |= (uint)EConnectionDir.Left;

        UDebug.Print($"Flag 1 : LeftRight {flag1} | Flag 2 : LeftRightUp {flag2} | Flag 3 : None {flag3} ");
        UDebug.Print($"Flag 1 & Flag 3 : {flag1 ^ flag3} ");
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
