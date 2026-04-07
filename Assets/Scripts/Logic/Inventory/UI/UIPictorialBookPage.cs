using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 도감 페이지 빌더.
/// 
/// 핵심
/// 1. 카테고리별 도감 엔트리 생성
/// 2. 해금 이벤트가 오면 강제로 다시 빌드
/// 3. 도감 열기 / 탭 전환 때도 강제로 인벤토리 동기화 후 재빌드 가능
/// </summary>
public class UIPictorialBookPage : BaseMono
{
    [Header("현재 카테고리")]
    [SerializeField] private string _category = "Animal";

    [Header("도감 시스템")]
    [SerializeField] private PictorialBookSystem _bookSystem;

    [Header("슬롯 생성 정보")]
    [SerializeField] private Transform _leftSlotRoot;
    [SerializeField] private Transform _rightSlotRoot;
    [SerializeField] private UIPictorialBookSlot _slotPrefab;
    [SerializeField] private bool _buildOnStart = true;

    [Header("페이지 슬롯 수")]
    [SerializeField] private int _leftPageCapacity = 20;
    [SerializeField] private int _rightPageCapacity = 20;

    private readonly List<UIPictorialBookSlot> _slots = new List<UIPictorialBookSlot>();
    private bool _isBuilt = false;

    public string CurrentCategory => _category;
    public PictorialBookSystem BookSystem => _bookSystem;

    private void OnEnable()
    {
        BindBookSystemEvent();
    }

    private void OnDisable()
    {
        UnbindBookSystemEvent();
    }

    private void Start()
    {
        BindBookSystemEvent();

        if (_buildOnStart)
        {
            ForceFullRefresh();
        }
    }

    public void SetCategory(string category, bool rebuildNow = true)
    {
        string normalizedCategory = NormalizeCategory(category);

        if (string.IsNullOrWhiteSpace(normalizedCategory))
        {
            Debug.LogWarning("[UIPictorialBookPage] category가 비어 있습니다.");
            return;
        }

        bool isChanged = !string.Equals(_category, normalizedCategory, StringComparison.OrdinalIgnoreCase);
        _category = normalizedCategory;

        if (rebuildNow)
        {
            if (isChanged || !_isBuilt)
            {
                ForceFullRefresh();
            }
            else
            {
                RefreshAll();
            }
        }
    }

    /// <summary>
    /// 가장 강한 갱신 방식
    /// 1. 현재 인벤토리에서 다시 도감 해금 동기화
    /// 2. 현재 카테고리 슬롯을 통째로 다시 생성
    /// </summary>
    public void ForceFullRefresh()
    {
        if (_bookSystem != null)
        {
            _bookSystem.SyncFromPlayerInventory();
        }

        Rebuild();
    }

    public void Rebuild()
    {
        _isBuilt = false;

        if (_bookSystem == null)
        {
            Debug.LogWarning("[UIPictorialBookPage] PictorialBookSystem이 연결되지 않았습니다.");
            return;
        }

        if (_leftSlotRoot == null && _rightSlotRoot == null)
        {
            Debug.LogWarning("[UIPictorialBookPage] 슬롯 루트가 연결되지 않았습니다.");
            return;
        }

        if (_slotPrefab == null)
        {
            Debug.LogWarning("[UIPictorialBookPage] SlotPrefab이 연결되지 않았습니다.");
            return;
        }

        ClearSlots();

        List<PictorialBookEntry> entries = _bookSystem.GetEntriesByCategory(_category);

        Debug.Log($"[UIPictorialBookPage] Rebuild category={_category}, entries={(entries == null ? 0 : entries.Count)}");

        if (entries == null || entries.Count == 0)
        {
            return;
        }

        int leftCapacity = Mathf.Max(0, _leftPageCapacity);
        int rightCapacity = Mathf.Max(0, _rightPageCapacity);
        int totalCapacity = leftCapacity + rightCapacity;

        int createCount = entries.Count;
        if (totalCapacity > 0)
        {
            createCount = Mathf.Min(entries.Count, totalCapacity);
        }

        for (int i = 0; i < createCount; i++)
        {
            PictorialBookEntry entry = entries[i];
            if (entry == null)
            {
                continue;
            }

            Transform parent = GetParentByIndex(i, leftCapacity, rightCapacity);
            if (parent == null)
            {
                continue;
            }

            UIPictorialBookSlot slot = Instantiate(_slotPrefab, parent);
            slot.SetData(_bookSystem, entry);
            _slots.Add(slot);
        }

        _isBuilt = true;
    }

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
        Debug.Log($"[UIPictorialBookPage] OnDiscovered itemId={itemId}, currentCategory={_category}");
        ForceFullRefresh();
    }

    private void BindBookSystemEvent()
    {
        if (_bookSystem == null)
        {
            return;
        }

        _bookSystem.OnDiscovered -= HandleDiscovered;
        _bookSystem.OnDiscovered += HandleDiscovered;
    }

    private void UnbindBookSystemEvent()
    {
        if (_bookSystem == null)
        {
            return;
        }

        _bookSystem.OnDiscovered -= HandleDiscovered;
    }

    private Transform GetParentByIndex(int index, int leftCapacity, int rightCapacity)
    {
        if (_leftSlotRoot != null && index < leftCapacity)
        {
            return _leftSlotRoot;
        }

        if (_rightSlotRoot != null && index >= leftCapacity && index < leftCapacity + rightCapacity)
        {
            return _rightSlotRoot;
        }

        if (_leftSlotRoot != null && _rightSlotRoot == null)
        {
            return _leftSlotRoot;
        }

        if (_rightSlotRoot != null && _leftSlotRoot == null)
        {
            return _rightSlotRoot;
        }

        return null;
    }

    private void ClearSlots()
    {
        ClearChildren(_leftSlotRoot);
        ClearChildren(_rightSlotRoot);
        _slots.Clear();
    }

    private void ClearChildren(Transform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    private string NormalizeCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return string.Empty;
        }

        return category.Trim();
    }
}
