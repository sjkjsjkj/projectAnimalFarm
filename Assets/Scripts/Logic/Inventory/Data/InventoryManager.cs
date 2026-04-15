using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 인벤토리들 (플레이어 인벤 / 창고 / 상점 등) 들을 관리하는 매니저 
/// </summary>
public class InventoryManager : GlobalSingleton<InventoryManager>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("프리팹")]
    //[SerializeField] private PlayerInventoryUI _playerInventoryPrefab;
    //[SerializeField] private PlayerInventoryUI _playerInventoryPrefab_;
    [SerializeField] private UIPlayerInventory _playerInventoryPrefab;
    [SerializeField] private StorageUI _storagePrefab;
    [SerializeField] private FoodBoxUI _foodBoxPrefab;
    [SerializeField] private Transform _inventoriesCanvasTr;

    [Header("테스트")]
    [SerializeField] private int _inventorySize;
    [SerializeField] private KeyCode _inventoryKeycode = KeyCode.I;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private bool _isPlayerInvenInit = false;
    private bool _isSettingFinish;

    protected float _reUseTimer = 0.2f;

    private UIPlayerInventory _playerInventoryUI;
    private StorageUI _storageUI;
    private FoodBoxUI _foodBoxUI;

    private List<Inventory> _inventoryList;

    private int _inventoryCount = 0;

    private int _currentOtherInventoryeId=-1;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Inventory PlayerInventory => _inventoryList[0];
    public UIPlayerInventory PlayerInvenUI => _playerInventoryUI;
    public StorageUI StorageUI => _storageUI;
    public FoodBoxUI FoodBoxUI => _foodBoxUI;

    public List<Inventory> Inventories => _inventoryList;

    public bool CanReUse => _reUseTimer >= 0.5f;
    public bool IsSettingFinish => _isSettingFinish;
    public Transform GlobalCanvas => _inventoriesCanvasTr;
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
        //_playerInventoryPrefab = Resources.Load<PlayerInventoryUI>("BootPrefab/PlayerInventoryUIPrefab");
        _playerInventoryPrefab = Resources.Load<UIPlayerInventory>("Prefab/UIInventory");
        _storagePrefab = Resources.Load<StorageUI>("Prefab/UIStorage");
        _foodBoxPrefab = Resources.Load<FoodBoxUI>("Prefab/UIFoodBox");

        StartCoroutine(SceneLoadWaitCoroutine());
    }

    private IEnumerator SceneLoadWaitCoroutine()
    {
        while (true)
        {
            yield return null;
            GameObject tempCanvas = UObject.Find(K.NAME_GLOBAL_CANVAS_ROOT);
            if (tempCanvas != null)
            {
                _inventoriesCanvasTr = tempCanvas.transform;
                break;
            }
        }
        MakeInventoryUIs();//인벤토리 UI들 생성 (인벤 / 창고 / 상점 각각 하나씩)
        MakeNewInventory(_inventorySize, EInventoryType.PlayerInventory); // 가장 먼저 플레이어의 인벤토리 생성.
        _isSettingFinish = true;
    }

    //각 UI들을 생성하는 메서드.
    //이곳에서 생성된 UI로 각종 창고와 상점 / 플레이어의 인벤토리 UI를 보여 줌.
    private void MakeInventoryUIs()
    {
        if (_playerInventoryPrefab == null)
        {
            UDebug.Print("Global Prefab : PlayerInvenUI 찾을 수 없음. 확인", LogType.Assert);
            return;
        }
        if (_storagePrefab == null)
        {
            UDebug.Print("Global Prefab : StorageUI 찾을 수 없음. 확인", LogType.Assert);
            return;
        }
        if (_foodBoxPrefab == null) 
        {
            UDebug.Print("Global Prefab : FoodBoxUI 찾을 수 없음. 확인", LogType.Assert);
            return;
        }

        //인벤토리 UI 활성화
        _playerInventoryUI = Instantiate(_playerInventoryPrefab);
        _playerInventoryUI.SetSize(K.PLAYER_INVENTORY_SIZE);
        _playerInventoryUI.transform.SetParent(_inventoriesCanvasTr);
        _playerInventoryUI.transform.localPosition = new Vector3(-450, 0);
        _playerInventoryUI.gameObject.SetActive(false);
        // 재조정
        RectTransform rectTr = _playerInventoryUI.GetComponent<RectTransform>();
        if (rectTr != null)
        {
            rectTr.localScale = Vector3.one;
            //rectTr.anchoredPosition = Vector2.zero;
        }

        //창고 UI 활성화
        _storageUI = Instantiate(_storagePrefab);
        _storageUI.SetSize(K.STORAGE_INVENTORY_SIZE);
        _storageUI.transform.SetParent(_inventoriesCanvasTr);
        _storageUI.transform.localPosition = new Vector3(450, 0);
        _storageUI.gameObject.SetActive(false);
        // 재조정
        rectTr = _storageUI.GetComponent<RectTransform>();
        if (rectTr != null)
        {
            rectTr.localScale = Vector3.one;
            //rectTr.anchoredPosition = Vector2.zero;
        }

        //먹이통 UI 활성화
        _foodBoxUI = Instantiate(_foodBoxPrefab);
        _foodBoxUI.SetSize(K.FOODBOX_INVENTORY_SIZE);
        _foodBoxUI.transform.SetParent(_inventoriesCanvasTr);
        _foodBoxUI.transform.localPosition = new Vector3(450, 0);
        _foodBoxUI.gameObject.SetActive(false);
        // 재조정
        rectTr = _foodBoxUI.GetComponent<RectTransform>();
        if (rectTr != null)
        {
            rectTr.localScale = Vector3.one;
            //rectTr.anchoredPosition = Vector2.zero;
        }
    }
    //각종 InteractObject에서 새로운 인벤토리 생성요청이 들어오면 이것을 실행하여 새로운 인벤토리를 생성함.
    public int RequestNewInventory(int newInventorySize, EInventoryType newInventoryType)
    {
        MakeNewInventory(newInventorySize, newInventoryType);
        //UDebug.Print($"current InventoryManager's Size : {_inventoryList.Count}");
        return _inventoryList.Count-1;
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
                //Debug.Log($"플레이어 인벤토리 생성 | CurInvenCount : {_inventoryCount}");
                //타입에 맞는 UI에 데이터 넣기.
                //플레이어의 인벤토리는 세상에 단 한개임으로 바로 데이터를 집어 넣는 것이 관리하기 편할 것이라고 판단.
                break;
            case EInventoryType.Storage:

                Inventory storageInventory = new Inventory(newInventorySize, EInventoryType.Storage, _inventoryList.Count);
                _inventoryList.Add(storageInventory);
                //Debug.Log($"플레이어 창고 생성 | : {_inventoryList .Count-1+ "번째 인벤토리"}");
                break;
            case EInventoryType.FoodBox:
                FoodBox foodBoxInventory = new FoodBox(newInventorySize, EInventoryType.FoodBox, _inventoryList.Count);
                _inventoryList.Add(foodBoxInventory);
                //Debug.Log($"플레이어 창고 생성 | : {_inventoryList.Count - 1 + "번째 인벤토리"}");
                break;
            default:
                _inventoryList.Add(new Inventory(newInventorySize, invenType, _inventoryList.Count));
                break;
        }
        _inventoryList[_inventoryList.Count - 1].OnChangeSlot -= NotifyRequestRefreshUISlot;
        _inventoryList[_inventoryList.Count - 1].OnChangeSlot += NotifyRequestRefreshUISlot;

        _inventoryList[_inventoryList.Count - 1].OnChangeSlots -= NotiftyRequestRefreshUI;
        _inventoryList[_inventoryList.Count - 1].OnChangeSlots += NotiftyRequestRefreshUI;
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
                _storageUI.RefreshInventoryUI(invenSlot.SlotIdx, invenSlot);
                break;
            case EInventoryType.FoodBox:
                _foodBoxUI.RefreshInventoryUI(invenSlot.SlotIdx, invenSlot);
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
                _storageUI.RefreshInventoryUI(inventory);
                break;
            case EInventoryType.FoodBox:
                _foodBoxUI.RefreshInventoryUI(inventory);
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
        _reUseTimer = 0;
        if (_inventoriesCanvasTr == null)
        {
            _inventoriesCanvasTr = UObject.Find(K.NAME_GLOBAL_CANVAS_ROOT).transform;
        }
        switch(invenType)
        {
            case EInventoryType.PlayerInventory:
                //PlayerInvenUI.SetToggleUI();
                //PlayerInvenUI_.OpenUI();
                _playerInventoryUI.SetToggleUI();
                break;
            case EInventoryType.Storage:
                OnChestOpen.Publish();
                _currentOtherInventoryeId = id;
                _storageUI.SetCurrentOpenInventoryId(id);
                _storageUI.RefreshInventoryUI(_inventoryList[id]);
                _storageUI.SetToggleUI();
                break;
            case EInventoryType.FoodBox:
                OnFeedBoxOpen.Publish();
                _currentOtherInventoryeId = id;
                _foodBoxUI.SetCurrentOpenInventoryId(id);
                _foodBoxUI.RefreshInventoryUI(_inventoryList[id]);
                _foodBoxUI.SetToggleUI();
                break;
        }
    }
    public void ChangeItemInvenNStorage(int fromInvenIdx, int fromSlotIdx, int toInvenIdx, int toSlotIdx)
    {
        if(fromInvenIdx == fromSlotIdx && toInvenIdx == toSlotIdx)
        {
            return;
        }
        //UDebug.Print($"현재 옮기려고 하는 인벤토리의 타입은 : {_inventoryList[toInvenIdx].InvenType}");
        if (_inventoryList[toInvenIdx] is FoodBox foodBoxInven)
        {
            //UDebug.Print("지금 먹이통으로 아이템을 이동하려 합니다.");
            if (!foodBoxInven.CheckItemType(_inventoryList[fromInvenIdx].InventorySlots[fromSlotIdx].ItemSO))
            {
                return;
            }
        }
        //Inventory Slot이 구조체라 받는 대상은 항상 호출해야하기 때문에 의미 없을 것 같아서 캐싱은 하지 않겠음.
        if (_inventoryList[toInvenIdx].InventorySlots[toSlotIdx].IsEmpty)
        {
            //UDebug.Print("빈 곳으로 이동");
            _inventoryList[toInvenIdx].SetItemSlot(_inventoryList[fromInvenIdx].InventorySlots[fromSlotIdx], toSlotIdx);
            _inventoryList[fromInvenIdx].ItemSlotClear(fromSlotIdx);

            return;
        }
        //UDebug.Print("서로 교환");
        InventorySlot tempInvenslot = _inventoryList[fromInvenIdx].InventorySlots[fromSlotIdx];

        _inventoryList[fromInvenIdx].SetItemSlot(_inventoryList[toInvenIdx].InventorySlots[toSlotIdx], fromSlotIdx);
        _inventoryList[toInvenIdx].SetItemSlot(tempInvenslot,toSlotIdx);
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
        _reUseTimer += Time.deltaTime;
    }
}
