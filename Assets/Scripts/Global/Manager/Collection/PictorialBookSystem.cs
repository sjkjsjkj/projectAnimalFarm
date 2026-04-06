using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도감 해금 상태를 관리하는 시스템.
/// 
/// 중요
/// - InventorySlot을 null 비교하지 않는다.
/// - 프로젝트에 따라 InventorySlot이 struct처럼 취급될 수 있어
///   null 비교에서 컴파일 에러가 날 수 있기 때문이다.
/// </summary>
public class PictorialBookSystem : BaseMono
{
    [Header("도감 원본 데이터")]
    [SerializeField] private SheetItemDatabase _sheetItemDatabase;

    [Header("옵션")]
    [SerializeField] private bool _syncInventoryOnStart = true;
    [SerializeField] private bool _useLog = true;

    private readonly HashSet<string> _discoveredItemIds = new HashSet<string>();

    public event Action<string> OnDiscovered;
    public int DiscoveredCount => _discoveredItemIds.Count;

    private void Start()
    {
        if (_syncInventoryOnStart)
        {
            SyncFromPlayerInventory();
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

    public void SyncFromPlayerInventory()
    {
        if (InventoryManager.Ins == null)
        {
            if (_useLog)
            {
                Debug.LogWarning("[PictorialBookSystem] InventoryManager.Ins가 null이라 초기 동기화를 건너뜁니다.");
            }
            return;
        }

        if (InventoryManager.Ins.PlayerInventory == null)
        {
            if (_useLog)
            {
                Debug.LogWarning("[PictorialBookSystem] PlayerInventory가 null이라 초기 동기화를 건너뜁니다.");
            }
            return;
        }

        Inventory playerInventory = InventoryManager.Ins.PlayerInventory;
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

    public void ClearDiscoveries()
    {
        _discoveredItemIds.Clear();

        if (_useLog)
        {
            Debug.Log("[PictorialBookSystem] 도감 해금 상태를 초기화했습니다.");
        }
    }
}
