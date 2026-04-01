using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도감 해금 여부를 관리하는 시스템
/// 아이템을 처음 획득하면 해당 ID를 도감에 등록한다.
/// </summary>
public class PictorialBookSystem : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private SheetItemDatabase _database;
    [SerializeField] private bool _logOnUnlock = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    // itemId -> discovered
    private readonly HashSet<string> _discoveredSet = new HashSet<string>();
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 도감 해금 시도
    /// 이미 해금된 경우 false 반환
    /// </summary>
    public bool TryDiscover(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogWarning("[PictorialBook] itemId가 비어 있습니다.");
            return false;
        }

        if (_database != null && !_database.TryGetRow(itemId, out SheetItemRow row))
        {
            Debug.LogWarning($"[PictorialBook] DB에 없는 itemId입니다: {itemId}");
            return false;
        }

        if (_discoveredSet.Contains(itemId))
        {
            return false;
        }

        _discoveredSet.Add(itemId);

        if (_logOnUnlock)
        {
            string itemName = itemId;

            if (_database != null && _database.TryGetRow(itemId, out SheetItemRow itemRow))
            {
                itemName = itemRow.Name;
            }

            Debug.Log($"[PictorialBook] 새 도감 해금: {itemName} ({itemId})");
        }

        return true;
    }

    /// <summary>
    /// 도감 해금 여부 확인
    /// </summary>
    public bool IsDiscovered(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        return _discoveredSet.Contains(itemId);
    }

    /// <summary>
    /// 전체 해금 수
    /// </summary>
    public int GetDiscoveredCount()
    {
        return _discoveredSet.Count;
    }
    #endregion
}
