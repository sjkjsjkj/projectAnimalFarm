using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 인벤토리들 (플레이어 인벤 / 창고 / 상점 등) 들을 관리하는 매니저 
/// </summary>
public class InventoryManager : Singleton<InventoryManager>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("프리팹")]
    [SerializeField] private PlayerInventoryUI _playerInventoryPrefab;
    [SerializeField] private InventoryUI _storagePrefab;
    [SerializeField] private InventoryUI _shopPrefab;
    [SerializeField] private Transform _inventoriesCanvasTr;

    [Header("테스트")]
    [SerializeField] private int _inventorySize;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private bool _isPlayerInvenInit = false;
    private Inventory _playerInventory;
    private PlayerInventoryUI _playerInventoryUI;
    private Dictionary<int, Inventory> _storageList;
    private Dictionary<int, Inventory> _shopList;
    private int _storageCount;
    private int _shopCount;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Inventory PlayerInventory => _playerInventory;
    public PlayerInventoryUI PlayerInvenUI => _playerInventoryUI;
    //public InventoryUI
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }
        _storageCount = _shopCount = 0;
        _storageList = new Dictionary<int, Inventory>();
        _shopList = new Dictionary<int, Inventory>();

        //MakeNewInventory(15, EInventoryType.PlayerInventory); // 게임 시작시 자동으로 플레이어의 인벤토리 생성.

        TestFunction();


        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    /// <summary>
    /// 테스트용 메서드
    /// </summary>
    public void TestFunction()
    {
        MakeNewInventory(_inventorySize, EInventoryType.PlayerInventory);
    }

    /// <summary>
    /// 새로운 인벤토리를 만드는 메서드 (새로운 창고나 저장공간이 생길 때 마다 이것으로 추가)
    /// </summary>
    /// <param name="inventoryIndex"></param>
    public void MakeNewInventory(int newInventorySize, EInventoryType invenType)
    {
        switch(invenType)
        {
            case EInventoryType.PlayerInventory:
                if (_isPlayerInvenInit) return;
                _playerInventory = new Inventory(newInventorySize);
                _playerInventoryUI = Instantiate(_playerInventoryPrefab);
                _playerInventoryUI.SetInfo(newInventorySize);
                _playerInventoryUI.transform.SetParent(_inventoriesCanvasTr);
                _playerInventoryUI.transform.localPosition = new Vector3(-300, 0);
                break;
            case EInventoryType.Storage:
                _storageList.Add(_storageCount++, new Inventory(newInventorySize));
                    break;
            case EInventoryType.Shop:
                _shopList.Add(_shopCount++, new Inventory(newInventorySize));
                break;
        }
    }

    /// <summary>
    /// 외부에서 인벤토리를 UI를 On할때 불러와질 메서드.
    /// 이곳에서 인벤토리 타입에 따라 열리는 형식과 목록이 달라질 예정.
    /// </summary>
    public void InventoryUIActiveOn(int id, EInventoryType invenType) 
    {
        switch(invenType)
        {
            case EInventoryType.PlayerInventory:
                PlayerInvenUI.gameObject.SetActive(true);
                break;
            case EInventoryType.Storage:

                break;
            case EInventoryType.Shop:

                break;
        }
    }
    #endregion
}
