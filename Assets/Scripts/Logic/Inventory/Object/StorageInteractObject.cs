using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어가 직접 창고를 열 때 인터랙션 할 대상입니다.
/// </summary>
public class StorageInteractObject : BaseMono, IInteractable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("창고 정보")]
    [SerializeField] private int _storageIdx; //테스트용

    [Header("자동 닫힘")]
    [SerializeField] private float _autoCloseDistance = 3.0f; // 우선 3으로 임의 설정했슴다. 

    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    private GameObject _currentPlayer;
    private bool _isStorageOpen;

    private InventoryManager _inventoryManager;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool CanInteract(GameObject player)
    {
        //throw new System.NotImplementedException();
        UDebug.Print($"current Reuse state : {_inventoryManager.CanReUse}");
        return _inventoryManager.CanReUse;
    }

    public string GetMessage()
    {
        return "상자 열려요~";
    }

    public void Interact(GameObject player)
    {
        UDebug.Print("창고야 열려라");

        StorageUI storageUI = _inventoryManager.StorageUI;

        UDebug.Print($"{_storageIdx}번째 인벤토리(창고) 오픈!");


        _inventoryManager.InventoryUIToggle(_storageIdx, EInventoryType.Storage);

        _currentPlayer = player;
        _isStorageOpen = !_isStorageOpen;

        //storageUI.SetToggleUI();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private IEnumerator CoWaitLoadInventoryManagerSetting()
    {
        _inventoryManager = InventoryManager.Ins;
        while (true)
        {
            if(_inventoryManager.IsSettingFinish)
            {
                break;
            }
            yield return null;

            _storageIdx = _inventoryManager.RequestNewInventory(K.STORAGE_INVENTORY_SIZE, EInventoryType.Storage);
        }
    }

    private void CheckAutoClose()
    {
        if (_isStorageOpen == false)
        {
            return;
        }

        if (_currentPlayer == null)
        {
            ForceCloseStorage();
            return;
        }

        float distance = Vector3.Distance(transform.position, _currentPlayer.transform.position);
        if (distance > _autoCloseDistance)
        {
            ForceCloseStorage();
        }
    }

    private void ForceCloseStorage()
    {
        if (_isStorageOpen == false)
        {
            return;
        }

        _inventoryManager.InventoryUIToggle(_storageIdx, EInventoryType.Storage);

        _isStorageOpen = false;
        _currentPlayer = null;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        StartCoroutine(CoWaitLoadInventoryManagerSetting());
    }

    private void Update()
    {
        CheckAutoClose();
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
