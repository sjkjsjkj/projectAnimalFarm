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
        //UDebug.Print("창고야 열려라");

        StorageUI storageUI = _inventoryManager.StorageUI;

        //UDebug.Print($"{_storageIdx}번째 인벤토리(창고) 오픈!");

        
        _inventoryManager.InventoryUIToggle(_storageIdx, EInventoryType.FoodBox);
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
        StartCoroutine(CoWaitLoadInventoryManagerSetting());

       
    }
    #endregion
}
