using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 구글시트에서 읽어온 아이템들을 저장하는 런타임 데이터베이스
/// 인벤토리 / 도감 / UI가 여기서 공통으로 데이터를 가져간다.
/// </summary>
public class SheetItemDatabase : BaseMono
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly Dictionary<string, SheetItemRow> _itemTable = new Dictionary<string, SheetItemRow>();
    private readonly List<SheetItemRow> _allItems = new List<SheetItemRow>();
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public IReadOnlyList<SheetItemRow> AllItems => _allItems;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 새로운 아이템 데이터를 추가한다.
    /// 이미 같은 ID가 있으면 덮어쓰지 않고 무시한다.
    /// </summary>
    public bool TryAddRow(SheetItemRow row)
    {
        if (row == null)
        {
            Debug.LogWarning("[SheetItemDatabase] row가 null입니다.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(row.id))
        {
            Debug.LogWarning("[SheetItemDatabase] row.Id가 비어 있습니다.");
            return false;
        }

        if (_itemTable.ContainsKey(row.id))
        {
            Debug.LogWarning($"[SheetItemDatabase] 중복 ID 발견: {row.id}");
            return false;
        }

        _itemTable.Add(row.id, row);
        _allItems.Add(row);
        return true;
    }

    /// <summary>
    /// ID로 아이템 데이터를 가져온다.
    /// </summary>
    public bool TryGetRow(string id, out SheetItemRow row)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            row = null;
            return false;
        }

        return _itemTable.TryGetValue(id, out row);
    }

    /// <summary>
    /// 특정 카테고리 아이템만 반환한다.
    /// </summary>
    public List<SheetItemRow> GetRowsByCategory(string category)
    {
        List<SheetItemRow> result = new List<SheetItemRow>();

        if (string.IsNullOrWhiteSpace(category))
        {
            return result;
        }

        for (int i = 0; i < _allItems.Count; i++)
        {
            if (_allItems[i] != null && _allItems[i].category == category)
            {
                result.Add(_allItems[i]);
            }
        }

        return result;
    }

    /// <summary>
    /// 런타임 DB 초기화
    /// </summary>
    public void Clear()
    {
        _itemTable.Clear();
        _allItems.Clear();
    }
    #endregion
}
