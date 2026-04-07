using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도감 해금 상태 관리 시스템.
/// 
/// 역할
/// 1. 인벤토리에 들어간 itemId를 도감 해금 목록에 저장
/// 2. 도감 DB에서 카테고리별 목록 반환
/// 3. UI 갱신 이벤트 전달
/// </summary>
public class PictorialBookSystem : BaseMono
{
    [Header("도감 데이터베이스")]
    [SerializeField] private PictorialBookDatabaseSO _bookDatabase;

    [Header("옵션")]
    [SerializeField] private bool _syncInventoryOnStart = true;
    [SerializeField] private bool _useLog = true;

    [Header("초기화 대기 설정")]
    [SerializeField] private float _inventoryWaitTimeout = 10f;

    private readonly HashSet<string> _discoveredItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public event Action<string> OnDiscovered;

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
        string normalizedItemId = NormalizeText(itemId);

        if (string.IsNullOrWhiteSpace(normalizedItemId))
        {
            Debug.LogWarning("[PictorialBookSystem] itemId가 비어 있습니다.");
            return false;
        }

        if (_discoveredItemIds.Contains(normalizedItemId))
        {
            return false;
        }

        _discoveredItemIds.Add(normalizedItemId);

        if (_useLog)
        {
            Debug.Log($"[PictorialBookSystem] 도감 해금: {normalizedItemId}");
        }

        OnDiscovered?.Invoke(normalizedItemId);
        return true;
    }

    public bool IsDiscovered(string itemId)
    {
        string normalizedItemId = NormalizeText(itemId);

        if (string.IsNullOrWhiteSpace(normalizedItemId))
        {
            return false;
        }

        return _discoveredItemIds.Contains(normalizedItemId);
    }

    public List<PictorialBookEntry> GetEntriesByCategory(string category)
    {
        if (_bookDatabase == null)
        {
            return new List<PictorialBookEntry>();
        }

        return _bookDatabase.GetEntriesByCategory(NormalizeText(category));
    }

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

    public void ClearDiscoveries()
    {
        _discoveredItemIds.Clear();

        if (_useLog)
        {
            Debug.Log("[PictorialBookSystem] 도감 해금 상태를 초기화했습니다.");
        }
    }

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

    private void SyncFromInventory(Inventory playerInventory)
    {
        if (playerInventory == null || playerInventory.InventorySlots == null)
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

            if (string.IsNullOrWhiteSpace(slot.ItemSO.Id))
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
        catch (Exception ex)
        {
            if (_useLog)
            {
                Debug.LogWarning($"[PictorialBookSystem] PlayerInventory 접근 중 예외 발생: {ex.Message}");
            }

            return false;
        }
    }

    private string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim();
    }
}
