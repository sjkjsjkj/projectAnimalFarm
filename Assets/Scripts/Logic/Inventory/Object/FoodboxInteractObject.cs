using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어가 직접 먹이통을 열 때 인터랙션 할 대상입니다.
/// </summary>
public class FoodboxInteractObject : BaseMono, IInteractable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("창고 정보")]
    [SerializeField] private int _storageIdx; //테스트용
    //[SerializeField] private BreedingArea _breedingArea;
    //[SerializeField] EInventoryType _inventoryType;

    [Header("자동 닫기 설정")]
    [SerializeField] private float _autoCloseDistance = 3.0f;
    [SerializeField] private bool _printAutoCloseLog = false;
    #endregion

    private InventoryManager _inventoryManager;

    public bool CanInteract(GameObject player)
    {
        //throw new System.NotImplementedException();
        //UDebug.Print($"current Reuse state : {_inventoryManager.CanReUse}");
        return _inventoryManager.CanReUse;
    }

    public string GetMessage()
    {
        return "먹이통 인터랙트";
    }

    public void Interact(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        if (HasValidInventoryManager(true) == false)
        {
            return;
        }
        //UDebug.Print("창고야 열려라");
        _inventoryManager.InventoryUIToggle(_storageIdx, EInventoryType.FoodBox);

        if (IsFoodBoxUiOpen())
        {
            StartAutoCloseTracking(player);
            return;
        }

        StopAutoCloseTracking();
        StorageUI storageUI = _inventoryManager.StorageUI;

        //UDebug.Print($"{_storageIdx}번째 인벤토리(창고) 오픈!");

        
        _inventoryManager.InventoryUIToggle(_storageIdx, EInventoryType.FoodBox);
    }

    /// <summary>
    /// InventoryManager가 유효한지 검사합니다.
    /// </summary>
    /// <param name="shouldPrintLog">실패 시 로그 출력 여부</param>
    /// <returns>유효 여부</returns>
    private bool HasValidInventoryManager(bool shouldPrintLog)
    {
        if (_inventoryManager == null)
        {
            if (shouldPrintLog)
            {
                UDebug.Print("FoodboxInteractObject: InventoryManager가 비어 있습니다.", LogType.Assert);
            }

            return false;
        }

        if (_inventoryManager.IsSettingFinish == false)
        {
            if (shouldPrintLog)
            {
                UDebug.Print("FoodboxInteractObject: InventoryManager 초기화가 아직 완료되지 않았습니다.", LogType.Warning);
            }

            return false;
        }

        if (_inventoryManager.FoodBoxUI == null)
        {
            if (shouldPrintLog)
            {
                UDebug.Print("FoodboxInteractObject: FoodBoxUI가 비어 있습니다.", LogType.Warning);
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// 현재 먹이통 UI가 열려 있는지 검사합니다.
    /// </summary>
    /// <returns>열림 여부</returns>
    private bool IsFoodBoxUiOpen()
    {
        if (HasValidInventoryManager(false) == false)
        {
            return false;
        }

        return _inventoryManager.FoodBoxUI.gameObject.activeInHierarchy;
    }

    /// <summary>
    /// 먹이통 UI 자동 닫기 추적을 시작합니다.
    /// </summary>
    /// <param name="player">상호작용한 플레이어</param>
    private void StartAutoCloseTracking(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        if (UIAutoCloseManager.Ins == null)
        {
            UDebug.Print("FoodboxInteractObject: UIAutoCloseManager가 씬에 없습니다.", LogType.Warning);
            return;
        }

        if (HasValidInventoryManager(true) == false)
        {
            return;
        }

        UIAutoCloseManager.Ins.StartTracking(
            player.transform,
            transform,
            _inventoryManager.FoodBoxUI,
            _inventoryManager.FoodBoxUI.gameObject,
            _autoCloseDistance,
            true,
            _printAutoCloseLog,
            "FoodBox");
    }

    /// <summary>
    /// 먹이통 UI 자동 닫기 추적을 중단합니다.
    /// </summary>
    private void StopAutoCloseTracking()
    {
        if (UIAutoCloseManager.Ins == null)
        {
            return;
        }

        UIAutoCloseManager.Ins.StopTracking();
    }

    private IEnumerator CoWaitLoadInventoryManagerSetting()
    {
        _inventoryManager = InventoryManager.Ins;
        while (true)
        {
            if (_inventoryManager.IsSettingFinish)
            {
                break;
            }
            yield return null;
            
            _storageIdx = _inventoryManager.RequestNewInventory(K.FOODBOX_INVENTORY_SIZE, EInventoryType.FoodBox);

            transform.Find("BreedingArea").GetComponent<BreedingArea>().SetInfo(_storageIdx, this.transform);
        }
    }

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(CoWaitLoadInventoryManagerSetting());

       
    }
    #endregion
}
