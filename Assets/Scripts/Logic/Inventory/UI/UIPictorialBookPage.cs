using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도감 한 페이지(UI) 빌더.
///
/// 역할
/// 1. category에 맞는 도감 데이터(Fish / Gather / Animal)를 가져온다.
/// 2. 슬롯 프리팹을 생성한다.
/// 3. 각 슬롯에 데이터를 넣는다.
/// 4. 도감 해금 이벤트가 오면 전체 슬롯을 다시 갱신한다.
/// </summary>
public class UIPictorialBookPage : BaseMono
{
    [SerializeField] private string _category;
    [SerializeField] private PictorialBookSystem _bookSystem;
    [SerializeField] private SpriteRegistry _spriteRegistry;
    [SerializeField] private Transform _slotRoot;
    [SerializeField] private UIPictorialBookSlot _slotPrefab;
    [SerializeField] private bool _buildOnStart = true;

    private readonly List<UIPictorialBookSlot> _slots = new List<UIPictorialBookSlot>();

    private void OnEnable()
    {
        if (_bookSystem != null)
        {
            _bookSystem.OnDiscovered -= HandleDiscovered;
            _bookSystem.OnDiscovered += HandleDiscovered;
        }
    }

    private void OnDisable()
    {
        if (_bookSystem != null)
        {
            _bookSystem.OnDiscovered -= HandleDiscovered;
        }
    }

    private void Start()
    {
        if (_buildOnStart)
        {
            Rebuild();
        }
    }

    /// <summary>
    /// 현재 category에 맞는 도감 슬롯을 다시 생성
    /// </summary>
    public void Rebuild()
    {
        if (_bookSystem == null)
        {
            Debug.LogWarning("[UIPictorialBookPage] PictorialBookSystem이 연결되지 않았습니다.");
            return;
        }

        if (_slotRoot == null)
        {
            Debug.LogWarning("[UIPictorialBookPage] SlotRoot가 연결되지 않았습니다.");
            return;
        }

        if (_slotPrefab == null)
        {
            Debug.LogWarning("[UIPictorialBookPage] SlotPrefab이 연결되지 않았습니다.");
            return;
        }

        ClearSlots();

        List<SheetItemRow> rows = _bookSystem.GetRowsByCategory(_category);
        if (rows == null || rows.Count == 0)
        {
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            SheetItemRow row = rows[i];
            if (row == null)
            {
                continue;
            }

            UIPictorialBookSlot slot = Instantiate(_slotPrefab, _slotRoot);
            slot.SetData(_bookSystem, _spriteRegistry, row);
            _slots.Add(slot);
        }
    }

    /// <summary>
    /// 현재 생성된 모든 슬롯 다시 갱신
    /// </summary>
    public void RefreshAll()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] == null)
            {
                continue;
            }

            _slots[i].Refresh();
        }
    }

    private void HandleDiscovered(string itemId)
    {
        RefreshAll();
    }

    private void ClearSlots()
    {
        if (_slotRoot == null)
        {
            _slots.Clear();
            return;
        }

        for (int i = _slotRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(_slotRoot.GetChild(i).gameObject);
        }

        _slots.Clear();
    }
}
