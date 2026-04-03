using System.Collections;
using UnityEngine;

/// <summary>
/// 아이템 획득 시 인벤토리와 도감을 동시에 갱신하는 중간 관리자
/// 다른 시스템은 이 클래스만 호출하면 된다.
/// </summary>
//public class ItemCollectionCoordinator : BaseMono
//{
//    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
//    [SerializeField] private Inventory _inventory;
//    [SerializeField] private PictorialBookSystem _pictorialBook;
//    #endregion

//    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
//    private IEnumerator InitSettingCoroutine()
//    {
//        while(true)
//        {
//            if(InventoryManager.Ins != null && InventoryManager.Ins.Inventories.Count>=1)
//            {
//                break;
//            }
//            yield return null;
//        }
//        UDebug.Print("인벤토리 연결 완료.");
//        _inventory = InventoryManager.Ins.PlayerInventory;
//    }
//    #endregion

//    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
//    public bool TryCollectItem(string itemId, int amount)
//    {
//        if (_inventory == null)
//        {
//            Debug.LogError("[ItemCollectionCoordinator] Inventory가 연결되지 않았습니다.");
//            return false;
//        }

//        //if (_pictorialBook == null)
//        //{
//        //    Debug.LogError("[ItemCollectionCoordinator] PictorialBook이 연결되지 않았습니다.");
//        //    return false;
//        //}

//        //string itemCategory;
//        //if (_database != null && _database.TryGetRow(itemId, out SheetItemRow itemRow))
//        //{
//        //    itemCategory = itemRow.Category;
//        //}

//        ItemSO tempItemSO = DatabaseManager.Ins.Item(itemId);

//        //bool added = _inventory.TryAddItem(itemId, amount);
//        bool added = _inventory.TryGetItem(tempItemSO, amount);

//        if (!added)
//        {
//            return false;
//        }

//        // 처음 얻었으면 도감 자동 해금
//        _pictorialBook.TryDiscover(itemId);

//        return true;
//    }
//    #endregion


//    #region ──────────────────────────── 메시지함수 ────────────────────────────
//    private void Awake()
//    {
//        StartCoroutine(InitSettingCoroutine());
//    }

//    #endregion
//}

public class ItemCollectionCoordinator : GlobalSingleton<ItemCollectionCoordinator>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private Inventory _inventory;
    [SerializeField] private PictorialBookSystem _pictorialBook;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    #endregion

    #region MyRegion

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        StartCoroutine(InitSettingCoroutine());
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    private IEnumerator InitSettingCoroutine()
    {
        while (true)
        {
            if (InventoryManager.Ins != null && InventoryManager.Ins.Inventories.Count >= 1)
            {
                break;
            }
            yield return null;
        }
        UDebug.Print("인벤토리 연결 완료.");
        _inventory = InventoryManager.Ins.PlayerInventory;
    }
   
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public bool TryCollectItem(string itemId, int amount)
    {
        if (_inventory == null)
        {
            Debug.LogError("[ItemCollectionCoordinator] Inventory가 연결되지 않았습니다.");
            return false;
        }

        //if (_pictorialBook == null)
        //{
        //    Debug.LogError("[ItemCollectionCoordinator] PictorialBook이 연결되지 않았습니다.");
        //    return false;
        //}

        //string itemCategory;
        //if (_database != null && _database.TryGetRow(itemId, out SheetItemRow itemRow))
        //{
        //    itemCategory = itemRow.Category;
        //}

        ItemSO tempItemSO = DatabaseManager.Ins.Item(itemId);

        //bool added = _inventory.TryAddItem(itemId, amount);
        bool added = _inventory.TryGetItem(tempItemSO, amount);

        if (!added)
        {
            return false;
        }

        // 처음 얻었으면 도감 자동 해금
        _pictorialBook.TryDiscover(itemId);

        return true;
    }
    #endregion

}

