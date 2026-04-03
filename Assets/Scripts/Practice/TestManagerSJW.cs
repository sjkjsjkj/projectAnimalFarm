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

    public void TestGetItemInvenDir()
    {
        ItemSO tempItemSO = DatabaseManager.Ins.Item(_itemId);

        InventoryManager.Ins.PlayerInventory.TryGetItem(tempItemSO);
    }
    public void TestGetItemInvenCoordinator()
    {
        _itemCoordinatior.TryCollectItem(_itemId, 1);
    }

    public void TestFarm()
    {
        _farmArea.TestFunction(_pos, _seedId.ToString());
    }

    public void TestCheckRecipe()
    {
        UDebug.Print("제작대 테스트");

        Workbench.Ins.OnChenageRecipe -= ShowCurrentRequireCondition;
        Workbench.Ins.OnChenageRecipe += ShowCurrentRequireCondition;

        Workbench.Ins.SetRecipe(_recipeSOs[_recipeIdx]);
    }

    public void ShowCurrentRequireCondition(WorkbenchReturnStruct[] curRequireCondition, bool canMake)
    {
        for (int i = 0; i < curRequireCondition.Length; i++)
        {
            UDebug.Print($"요구 아이템 {i} [{_recipeSOs[_recipeIdx].RequiedItems[i].Id}] : {curRequireCondition[i].CurHasCount} / {curRequireCondition[i].RequireCount} | 제작 가능 여부 : {curRequireCondition[i].IsCondition}");
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
        Workbench.Ins.MakeItem();

        Workbench.Ins.SetRecipe(_recipeSOs[_recipeIdx]);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Start()
    {
        _itemCoordinatior = GameObject.Find("Item_Collection_Coordinator").GetComponent<ItemCollectionCoordinator>();
    }
    #endregion
}
