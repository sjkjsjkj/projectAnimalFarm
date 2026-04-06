using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 구글 시트에서 읽어온 아이템 데이터를 런타임에서 보관하는 DB.
/// 
/// 이 스크립트의 역할
/// 1. GoogleSheetTsvLoader가 파싱한 데이터를 저장한다.
/// 2. itemId로 특정 행을 찾을 수 있게 한다.
/// 3. category(Fish / Gather / Animal) 기준으로 목록을 가져올 수 있게 한다.
/// 4. 낚시 / 도감 / UI가 같은 데이터를 함께 참조할 수 있게 한다.
/// 
/// 중요
/// - 이 스크립트는 "데이터 보관" 역할만 한다.
/// - 실제 TSV 파싱은 GoogleSheetTsvLoader가 담당한다.
/// - 실제 인벤토리 지급은 ItemCollectionCoordinator가 담당한다.
/// </summary>
public class SheetItemDatabase : BaseMono
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    /// <summary>
    /// itemId 기준 빠른 검색용 테이블
    /// </summary>
    private readonly Dictionary<string, SheetItemRow> _itemTable = new Dictionary<string, SheetItemRow>();

    /// <summary>
    /// 전체 아이템 순회용 리스트
    /// 
    /// 왜 따로 갖고 있나?
    /// - 낚시 후보 전체 검색
    /// - 도감 전체 슬롯 생성
    /// - 카테고리별 순회
    /// 같은 작업이 필요하기 때문
    /// </summary>
    private readonly List<SheetItemRow> _allItems = new List<SheetItemRow>();
    #endregion

    #region ─────────────────────────▶ 공개 프로퍼티 ◀─────────────────────────
    /// <summary>
    /// 전체 아이템 목록
    /// 외부에서는 읽기 전용으로만 사용
    /// </summary>
    public IReadOnlyList<SheetItemRow> AllItems => _allItems;

    /// <summary>
    /// 현재 등록된 총 아이템 개수
    /// </summary>
    public int Count => _allItems.Count;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 새로운 행 데이터를 DB에 추가
    /// 
    /// 반환값
    /// - true  : 등록 성공
    /// - false : null / 빈 ID / 중복 ID
    /// </summary>
    public bool AddRow(SheetItemRow row)
    {
        // null 방어
        if (row == null)
        {
            Debug.LogWarning("[SheetItemDatabase] row가 null입니다.");
            return false;
        }

        // ID가 비어 있으면 등록 불가
        if (string.IsNullOrWhiteSpace(row.id))
        {
            Debug.LogWarning("[SheetItemDatabase] row.id가 비어 있습니다.");
            return false;
        }

        // 같은 ID가 이미 있으면 중복 등록 방지
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
    /// itemId로 특정 행 찾기
    /// </summary>
    public bool TryGetRow(string id, out SheetItemRow row)
    {
        row = null;

        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        return _itemTable.TryGetValue(id, out row);
    }

    /// <summary>
    /// itemId로 특정 행을 바로 반환
    /// 못 찾으면 null 반환
    /// </summary>
    public SheetItemRow GetRowOrNull(string id)
    {
        if (TryGetRow(id, out SheetItemRow row))
        {
            return row;
        }

        return null;
    }

    /// <summary>
    /// category 기준으로 행 목록 반환
    /// 
    /// 예:
    /// - Fish
    /// - Gather
    /// - Animal
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
            SheetItemRow row = _allItems[i];

            if (row == null)
            {
                continue;
            }

            if (row.category == category)
            {
                result.Add(row);
            }
        }

        return result;
    }

    /// <summary>
    /// 특정 카테고리의 개수 반환
    /// </summary>
    public int GetCountByCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return 0;
        }

        int count = 0;

        for (int i = 0; i < _allItems.Count; i++)
        {
            SheetItemRow row = _allItems[i];

            if (row == null)
            {
                continue;
            }

            if (row.category == category)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 등록된 전체 데이터 초기화
    /// 
    /// 주로 TSV를 다시 로드할 때 사용
    /// </summary>
    public void Clear()
    {
        _itemTable.Clear();
        _allItems.Clear();
    }
    #endregion
}
