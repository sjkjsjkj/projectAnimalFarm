using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 모든 인벤토리들 (플레이어 인벤 / 창고 / 상점 등) 들을 관리하는 매니저 
/// </summary>
public class InventoryManager : Singleton<InventoryManager>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("프리팹")]
    [SerializeField] private PlayerInventoryUI _playerInventoryPrefab;
    [SerializeField] private StorageUI _storagePrefab;
    [SerializeField] private ShopUI _shopPrefab;
    [SerializeField] private Transform _inventoriesCanvasTr;

    [Header("테스트")]
    [SerializeField] private int _inventorySize;
    [SerializeField] private KeyCode _inventoryKeycode = KeyCode.I;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private bool _isPlayerInvenInit = false;

    private PlayerInventoryUI _playerInventoryUI;
    private StorageUI _storageUI;
    private ShopUI _shopUI;
    
    private List<Inventory> _inventoryList;

    private int _inventoryCount = 0;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Inventory PlayerInventory => _inventoryList[0];
    public PlayerInventoryUI PlayerInvenUI => _playerInventoryUI;
    public List<Inventory> Inventories => _inventoryList;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }
        _inventoryList = new List<Inventory>();

        InitSetting();

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }

    private void InitSetting()
    {
        _inventorySize = K.PLAYER_INVENTORY_SIZE;
        //TODO : 창고 / 상점 / 먹이통 생기면 사이즈 추가.
        CollectPrefab();
    }
    //Resources 안의 Prefab들을 수집하는 메서드.
    private void CollectPrefab()
    {
        _playerInventoryPrefab = Resources.Load<PlayerInventoryUI>("BootPrefab/PlayerInventoryUIPrefab");
        StartCoroutine(SceneLoadWaitCoroutine());
    }

    private IEnumerator SceneLoadWaitCoroutine()
    {
        while (true)
        {
            yield return null;
            GameObject tempCanvas = UObject.Find(K.NAME_UI_ROOT);
            if (tempCanvas != null)
            {
                _inventoriesCanvasTr = tempCanvas.transform;
                break;
            }
        }
        MakeInventoryUIs();//인벤토리 UI들 생성 (인벤 / 창고 / 상점 각각 하나씩)

        MakeNewInventory(_inventorySize, EInventoryType.PlayerInventory); // 가장 먼저 플레이어의 인벤토리 생성.
    }

    //각 UI들을 생성하는 메서드.
    //이곳에서 생성된 UI로 각종 창고와 상점 / 플레이어의 인벤토리 UI를 보여 줌.
    private void MakeInventoryUIs()
    {
        //추 후 UI가 생기면 활성화
        //if(_playerInventoryPrefab == null || _shopPrefab == null || _storagePrefab == null)
        //{
        //    UDebug.Print("인벤토리 UI 찾을 수 없음. 확인", LogType.Warning);
        //    return;
        //}

        //플레이어 인벤토리 UI 세팅
        _playerInventoryUI = Instantiate(_playerInventoryPrefab);
        _playerInventoryUI.SetInfo(_inventorySize);
        _playerInventoryUI.transform.SetParent(_inventoriesCanvasTr);
        _playerInventoryUI.transform.localPosition = new Vector3(-300, 0);
        _playerInventoryUI.gameObject.SetActive(false);

        //추 후 UI가 생기면 활성화
        ////창고 UI 세팅
        //_storageUI = Instantiate(_storagePrefab);
        //_storageUI.SetInfo(_storageInventoryUISize);
        //_storageUI.transform.SetParent(_inventoriesCanvasTr);
        //_storageUI.transform.localPosition = new Vector3(300, 0);
        //_storageUI.gameObject.SetActive(false);

        //추 후 UI가 생기면 활성화
        ////상점 UI 세팅
        //_shopUI = Instantiate(_shopPrefab);
        //_shopUI.SetInfo(_shopInventoryUISize);
        //_shopUI.transform.SetParent(_inventoriesCanvasTr);
        //_shopUI.transform.localPosition = new Vector3(300, 0);
        //_shopUI.gameObject.SetActive(false);
    }

    // 새로운 인벤토리를 만드는 메서드 (새로운 창고나 저장공간이 생길 때 마다 이것으로 추가)
    private void MakeNewInventory(int newInventorySize, EInventoryType invenType)
    {
        switch (invenType)
        {
            case EInventoryType.PlayerInventory:
                if (_isPlayerInvenInit) return;
                _isPlayerInvenInit = true;
                //데이터 인벤토리를 생성
                
                Inventory playerInventory = new Inventory(newInventorySize, EInventoryType.PlayerInventory, 0);
                //리스트 0번에 플레이어 인벤토리 넣기.
                _inventoryList.Add(playerInventory);
                Debug.Log($"플레이어 인벤토리 생성 | CurInvenCount : {_inventoryCount}");
                //타입에 맞는 UI에 데이터 넣기.
                //플레이어의 인벤토리는 세상에 단 한개임으로 바로 데이터를 집어 넣는 것이 관리하기 편할 것이라고 판단.
                break;
            default:
                _inventoryList.Add(new Inventory(newInventorySize, invenType, _inventoryCount));
                break;
        }
        _inventoryList[_inventoryCount].OnChangeSlot -= NotifyRequestRefreshUISlot;
        _inventoryList[_inventoryCount].OnChangeSlot += NotifyRequestRefreshUISlot;

        _inventoryList[_inventoryCount].OnChangeSlots -= NotiftyRequestRefreshUI;
        _inventoryList[_inventoryCount].OnChangeSlots += NotiftyRequestRefreshUI;

        _inventoryCount++;
    }
    /// <summary>
    /// 인벤토리에서 특정 슬롯의 변화가 일어났을 때 UI에게 알리기 위한 중간 함수.
    /// </summary>
    /// <param name="slotIdx"></param>
    /// <param name="ItemID"></param>
    /// <param name="inventoryIdx"></param>
    public void NotifyRequestRefreshUISlot(EInventoryType inventoryType, InventorySlot invenSlot)
    {
        switch(inventoryType)
        {
            case EInventoryType.PlayerInventory:
                _playerInventoryUI.RefreshInventoryUI(invenSlot.SlotIdx, invenSlot);
                break;
            case EInventoryType.Storage:
                break;
            case EInventoryType.Shop:
                break;
        }
    }
    public void NotiftyRequestRefreshUI(EInventoryType inventoryType, Inventory inventory)
    {
        switch (inventoryType)
        {
            case EInventoryType.PlayerInventory:
                _playerInventoryUI.RefreshInventoryUI(inventory);
                break;
            case EInventoryType.Storage:
                break;
            case EInventoryType.Shop:
                break;
        }
    }


    /// <summary>
    /// //여기서 실제 인벤토리들의 UI On / Off 호출
    /// 외부에서 인벤토리를 UI를 On할때 불러와질 메서드.
    /// 이곳에서 인벤토리 타입에 따라 열리는 형식과 목록이 달라질 예정.
    /// </summary>
    public void InventoryUIToggle(int id, EInventoryType invenType) 
    {
        if(_inventoriesCanvasTr == null)
        {
            _inventoriesCanvasTr = UObject.Find("Canvas").transform;
        }
        switch(invenType)
        {
            case EInventoryType.PlayerInventory:
                PlayerInvenUI.SetToggleUI();
                break;
            case EInventoryType.Storage:
                _storageUI.RefreshInventoryUI(_inventoryList[id]);
                _storageUI.SetToggleUI();
                break;
            case EInventoryType.Shop:
                _shopUI.RefreshInventoryUI(_inventoryList[id]);
                _shopUI.SetToggleUI();
                break;
        }
    }
    
    //테스트 용 메서드
    //UI On 키를 입력했는지 확인하는 메서드
    private void UIKeyInputHandle()
    {
        if(Input.GetKeyDown(_inventoryKeycode))
        {
            InventoryUIToggle(0, EInventoryType.PlayerInventory);
        }
    }
    #endregion

    private void Update()
    {
        UIKeyInputHandle();
    }
}
