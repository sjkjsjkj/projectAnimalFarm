using System.Collections;
using UnityEngine;

/// <summary>
/// 낚시 / 채집 / 수확 등 "아이템 획득"을 한 곳으로 모아 처리하는 클래스.
/// 
/// 역할
/// 1. 문자열 itemId를 실제 ItemSO로 변환
/// 2. 팀 메인 인벤토리(PlayerInventory)에 아이템 추가
/// 3. 아이템 획득 성공 시 도감 해금
/// 
/// 중요
/// - 팀원 인벤토리 스크립트는 수정하지 않는다.
/// - 다른 팀원 스크립트에서 ItemCollectionCoordinator.Ins 를 사용하고 있으므로
///   GlobalSingleton 형태를 유지한다.
/// </summary>
public class ItemCollectionCoordinator : GlobalSingleton<ItemCollectionCoordinator>
{
    [Header("도감 시스템")]
    [SerializeField] private PictorialBookSystem _pictorialBookSystem;

    [Header("로그")]
    [SerializeField] private bool _useLog = true;

    private bool _isInitialized = false;

    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        if (_pictorialBookSystem == null)
        {
            _pictorialBookSystem = FindObjectOfType<PictorialBookSystem>();
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 문자열 ID 기준으로 아이템 획득 처리
    /// </summary>
    public bool TryCollectItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogWarning("[ItemCollectionCoordinator] itemId가 비어 있습니다.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"[ItemCollectionCoordinator] amount가 0 이하입니다. itemId={itemId}, amount={amount}");
            return false;
        }

        if (DatabaseManager.Ins == null)
        {
            Debug.LogError("[ItemCollectionCoordinator] DatabaseManager.Ins가 null입니다.");
            return false;
        }

        if (!TryGetPlayerInventory(out Inventory playerInventory))
        {
            Debug.LogError("[ItemCollectionCoordinator] PlayerInventory를 찾지 못했습니다.");
            return false;
        }

        // 팀 DB에 이미 Item(string id) 진입점이 있으므로 이걸 그대로 사용
        ItemSO itemSo = DatabaseManager.Ins.Item(itemId);
        if (itemSo == null)
        {
            Debug.LogWarning($"[ItemCollectionCoordinator] DatabaseManager에서 ItemSO를 찾지 못했습니다. itemId={itemId}");
            return false;
        }

        return TryCollectItem(itemSo, amount, playerInventory);
    }

    /// <summary>
    /// ItemSO 기준으로 아이템 획득 처리
    /// </summary>
    public bool TryCollectItem(ItemSO itemSo, int amount = 1)
    {
        if (itemSo == null)
        {
            Debug.LogWarning("[ItemCollectionCoordinator] itemSo가 null입니다.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"[ItemCollectionCoordinator] amount가 0 이하입니다. itemId={itemSo.Id}, amount={amount}");
            return false;
        }

        if (!TryGetPlayerInventory(out Inventory playerInventory))
        {
            Debug.LogError("[ItemCollectionCoordinator] PlayerInventory를 찾지 못했습니다.");
            return false;
        }

        return TryCollectItem(itemSo, amount, playerInventory);
    }

    /// <summary>
    /// 기존 호출 이름을 유지하기 위한 래퍼
    /// </summary>
    public bool TryReceiveItem(string itemId, int amount = 1)
    {
        return TryCollectItem(itemId, amount);
    }

    private bool TryGetPlayerInventory(out Inventory playerInventory)
    {
        playerInventory = null;

        if (InventoryManager.Ins == null)
        {
            return false;
        }

        if (InventoryManager.Ins.Inventories == null)
        {
            return false;
        }

        if (InventoryManager.Ins.Inventories.Count <= 0)
        {
            return false;
        }

        playerInventory = InventoryManager.Ins.PlayerInventory;
        return playerInventory != null;
    }

    private bool TryCollectItem(ItemSO itemSo, int amount, Inventory playerInventory)
    {
        if (itemSo == null || playerInventory == null)
        {
            return false;
        }

        int successCount = 0;

        // 팀 Inventory 구조를 그대로 쓰기 위해 1개씩 반복 추가
        for (int i = 0; i < amount; i++)
        {
            bool result = playerInventory.TryGetItem(itemSo);
            if (!result)
            {
                break;
            }

            successCount++;
        }

        if (successCount <= 0)
        {
            if (_useLog)
            {
                Debug.LogWarning($"[ItemCollectionCoordinator] 인벤토리에 아이템 추가 실패: {itemSo.Id}");
            }

            return false;
        }

        if (_pictorialBookSystem != null)
        {
            _pictorialBookSystem.TryDiscover(itemSo.Id);
        }

        if (_useLog)
        {
            if (successCount == amount)
            {
                Debug.Log($"[ItemCollectionCoordinator] 아이템 획득 성공: {itemSo.Name} ({itemSo.Id}) x{successCount}");
            }
            else
            {
                Debug.LogWarning($"[ItemCollectionCoordinator] 일부만 획득 성공: {itemSo.Name} ({itemSo.Id}) x{successCount}/{amount}");
            }
        }

        return successCount == amount;
    }

    private void Start()
    {
        StartCoroutine(CoFindSheetItemDatabase());
    }

    private IEnumerator CoFindSheetItemDatabase()
    {
        while (_pictorialBookSystem != null)
        {
            _pictorialBookSystem = FindAnyObjectByType<PictorialBookSystem>();
            yield return null;
        }
    }
}
