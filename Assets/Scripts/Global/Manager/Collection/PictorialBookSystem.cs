using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도감 해금 상태를 관리하는 시스템.
/// 
/// 핵심 수정
/// - Start에서 바로 PlayerInventory를 읽지 않는다.
/// - InventoryManager가 실제로 플레이어 인벤토리를 만든 뒤에만 동기화한다.
/// </summary>
public class PictorialBookSystem : BaseMono
{
    [Header("도감 원본 데이터")]
    [SerializeField] private SheetItemDatabase _sheetItemDatabase;

    [Header("옵션")]
    [SerializeField] private bool _syncInventoryOnStart = true;
    [SerializeField] private bool _useLog = true;

    [Header("초기화 대기 설정")]
    [Tooltip("씬 시작 후 PlayerInventory가 준비될 때까지 기다릴 최대 시간(초)")]
    [SerializeField] private float _inventoryWaitTimeout = 10f;

    private readonly HashSet<string> _discoveredItemIds = new HashSet<string>();

    public event System.Action<string> OnDiscovered;
    public int DiscoveredCount => _discoveredItemIds.Count;

    private void Start()
    {
        if (_syncInventoryOnStart)
        {
            StartCoroutine(CoSyncFromPlayerInventoryWhenReady());
        }
    }

    public bool TryDiscover(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogWarning("[PictorialBookSystem] itemId가 비어 있습니다.");
            return false;
        }

        if (_discoveredItemIds.Contains(itemId))
        {
            return false;
        }

        _discoveredItemIds.Add(itemId);

        if (_useLog)
        {
            Debug.Log($"[PictorialBookSystem] 도감 해금: {itemId}");
        }

        OnDiscovered?.Invoke(itemId);
        return true;
    }

    public bool IsDiscovered(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        return _discoveredItemIds.Contains(itemId);
    }

    public List<SheetItemRow> GetRowsByCategory(string category)
    {
        if (_sheetItemDatabase == null)
        {
            return new List<SheetItemRow>();
        }

        return _sheetItemDatabase.GetRowsByCategory(category);
    }

    /// <summary>
    /// PlayerInventory가 실제로 준비될 때까지 기다렸다가 동기화
    /// </summary>
    private IEnumerator CoSyncFromPlayerInventoryWhenReady()
    {
        float elapsed = 0f;

        while (elapsed < _inventoryWaitTimeout)
        {
            if (TryGetPlayerInventorySafe(out Inventory playerInventory))
            {
                SyncFromInventory(playerInventory);
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_useLog)
        {
            Debug.LogWarning("[PictorialBookSystem] 제한 시간 내에 PlayerInventory가 준비되지 않아 초기 동기화를 건너뜁니다.");
        }
    }

    /// <summary>
    /// 외부에서 수동으로 다시 동기화할 때 사용
    /// </summary>
    public void SyncFromPlayerInventory()
    {
        if (!TryGetPlayerInventorySafe(out Inventory playerInventory))
        {
            if (_useLog)
            {
                Debug.LogWarning("[PictorialBookSystem] PlayerInventory를 찾지 못해 동기화를 건너뜁니다.");
            }
            return;
        }

        SyncFromInventory(playerInventory);
    }

    private void SyncFromInventory(Inventory playerInventory)
    {
        if (playerInventory == null)
        {
            return;
        }

        if (playerInventory.InventorySlots == null)
        {
            return;
        }

        for (int i = 0; i < playerInventory.InventorySlots.Length; i++)
        {
            InventorySlot slot = playerInventory.InventorySlots[i];

            if (slot.IsEmpty)
            {
                continue;
            }

            if (slot.ItemSO == null)
            {
                continue;
            }

            TryDiscover(slot.ItemSO.Id);
        }
    }

    private bool TryGetPlayerInventorySafe(out Inventory playerInventory)
    {
        playerInventory = null;

        if (InventoryManager.Ins == null)
        {
            return false;
        }

        try
        {
            playerInventory = InventoryManager.Ins.PlayerInventory;
            return playerInventory != null;
        }
        catch (System.ArgumentOutOfRangeException)
        {
            return false;
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            return false;
        }
        catch (System.Exception ex)
        {
            if (_useLog)
            {
                Debug.LogWarning($"[PictorialBookSystem] PlayerInventory 접근 중 예외 발생: {ex.Message}");
            }
            return false;
        }
    }

    public void ClearDiscoveries()
    {
        _discoveredItemIds.Clear();

        if (_useLog)
        {
            Debug.Log("[PictorialBookSystem] 도감 해금 상태를 초기화했습니다.");
        }
    }
}
