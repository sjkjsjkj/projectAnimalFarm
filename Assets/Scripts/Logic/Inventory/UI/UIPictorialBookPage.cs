using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 도감 페이지 빌더.
///
/// 수정 내용
/// 1. 엔트리 개수와 상관없이 페이지 슬롯 수만큼 항상 슬롯을 생성한다.
///    -> 물고기처럼 엔트리가 적은 카테고리도 빈 슬롯이 같이 보이게 함
/// 2. 페이지가 비어 있어도 1페이지는 유지해서 책 UI가 비어 보이지 않게 함
/// 3. 슬롯 클릭 / 페이지 이동 로직은 기존 방식 유지
/// </summary>
public class UIPictorialBookPage : BaseMono
{
    [Header("현재 카테고리")]
    [SerializeField] private string _category = "Animal";

    [Header("도감 시스템")]
    [SerializeField] private PictorialBookSystem _bookSystem;

    [Header("상세 정보 패널")]
    [SerializeField] private UIPictorialBookDetailPanel _detailPanel;

    [Header("슬롯 생성 정보")]
    [SerializeField] private Transform _leftSlotRoot;
    [SerializeField] private Transform _rightSlotRoot;
    [SerializeField] private UIPictorialBookSlot _slotPrefab;
    [SerializeField] private bool _buildOnStart = true;

    [Header("페이지 슬롯 수")]
    [SerializeField] private int _leftPageCapacity = 20;
    [SerializeField] private int _rightPageCapacity = 20;

    [Header("페이지 이동 버튼")]
    [SerializeField] private Button _prevPageButton;
    [SerializeField] private Button _nextPageButton;

    [Header("페이지 표시 텍스트 (선택)")]
    [SerializeField] private TMP_Text _pageIndexText;
    [SerializeField] private string _pageTextFormat = "{0} / {1}";

    [Header("옵션")]
    [SerializeField] private bool _hideDetailPanelOnPageChanged = true;
    [SerializeField] private bool _logEnabled = true;

    private readonly List<UIPictorialBookSlot> _slots = new List<UIPictorialBookSlot>();
    private List<PictorialBookEntry> _cachedEntries = new List<PictorialBookEntry>();

    private bool _isBuilt = false;
    private int _currentPageIndex = 0;

    public string CurrentCategory => _category;
    public PictorialBookSystem BookSystem => _bookSystem;
    public int CurrentPageIndex => _currentPageIndex;
    public int CurrentPageNumber => _currentPageIndex + 1;
    public int TotalPageCount => GetTotalPageCount(_cachedEntries != null ? _cachedEntries.Count : 0);

    private void OnEnable()
    {
        BindBookSystemEvent();
        RefreshNavigationState();
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
        else
        {
            RefreshNavigationState();
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

        if (isChanged)
        {
            _currentPageIndex = 0;

            if (_detailPanel != null)
            {
                _detailPanel.Hide();
            }
        }

        if (rebuildNow)
        {
            if (isChanged || !_isBuilt)
            {
                ForceFullRefresh();
            }
            else
            {
                RefreshAll();
                RefreshNavigationState();
            }
        }
    }

    /// <summary>
    /// 가장 강한 갱신 방식
    /// 1. 현재 인벤토리에서 다시 도감 해금 동기화
    /// 2. 현재 카테고리 슬롯을 다시 생성
    /// 3. 현재 페이지 인덱스를 유지하되 범위를 넘으면 자동 보정
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
            ClearSlots();
            _cachedEntries.Clear();
            _currentPageIndex = 0;
            RefreshNavigationState();
            return;
        }

        if (_leftSlotRoot == null && _rightSlotRoot == null)
        {
            Debug.LogWarning("[UIPictorialBookPage] 슬롯 루트가 연결되지 않았습니다.");
            ClearSlots();
            _cachedEntries.Clear();
            _currentPageIndex = 0;
            RefreshNavigationState();
            return;
        }

        if (_slotPrefab == null)
        {
            Debug.LogWarning("[UIPictorialBookPage] SlotPrefab이 연결되지 않았습니다.");
            ClearSlots();
            _cachedEntries.Clear();
            _currentPageIndex = 0;
            RefreshNavigationState();
            return;
        }

        int pageCapacity = GetPageCapacity();
        if (pageCapacity <= 0)
        {
            Debug.LogWarning("[UIPictorialBookPage] 페이지 슬롯 수가 0 이하입니다. Left/Right Capacity를 확인해주세요.");
            ClearSlots();
            _cachedEntries.Clear();
            _currentPageIndex = 0;
            RefreshNavigationState();
            return;
        }

        if (_hideDetailPanelOnPageChanged && _detailPanel != null)
        {
            _detailPanel.Hide();
        }

        ClearSlots();

        List<PictorialBookEntry> entries = _bookSystem.GetEntriesByCategory(_category);
        _cachedEntries = entries ?? new List<PictorialBookEntry>();

        int totalPageCount = GetTotalPageCount(_cachedEntries.Count);
        _currentPageIndex = Mathf.Clamp(_currentPageIndex, 0, totalPageCount - 1);

        int startIndex = _currentPageIndex * pageCapacity;
        int leftCapacity = Mathf.Max(0, _leftPageCapacity);
        int rightCapacity = Mathf.Max(0, _rightPageCapacity);

        if (_logEnabled)
        {
            Debug.Log(
                $"[UIPictorialBookPage] Rebuild category={_category}, totalEntries={_cachedEntries.Count}, " +
                $"page={CurrentPageNumber}/{totalPageCount}, startIndex={startIndex}, pageCapacity={pageCapacity}");
        }

        // 엔트리 수와 상관없이 페이지 슬롯 수만큼 항상 생성
        for (int localIndex = 0; localIndex < pageCapacity; localIndex++)
        {
            Transform parent = GetParentByIndex(localIndex, leftCapacity, rightCapacity);
            if (parent == null)
            {
                continue;
            }

            int entryIndex = startIndex + localIndex;
            PictorialBookEntry entry = entryIndex < _cachedEntries.Count ? _cachedEntries[entryIndex] : null;

            UIPictorialBookSlot slot = Instantiate(_slotPrefab, parent);
            slot.SetData(_bookSystem, entry, _detailPanel);
            _slots.Add(slot);
        }

        _isBuilt = true;
        RefreshNavigationState();
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

        RefreshNavigationState();
    }

    public void OnClickPrevPage()
    {
        if (CanMovePrevPage() == false)
        {
            return;
        }

        _currentPageIndex--;
        Rebuild();
    }

    public void OnClickNextPage()
    {
        if (CanMoveNextPage() == false)
        {
            return;
        }

        _currentPageIndex++;
        Rebuild();
    }

    public void GoToPage(int pageIndex)
    {
        int totalPageCount = GetTotalPageCount(_cachedEntries != null ? _cachedEntries.Count : 0);
        int clampedPageIndex = Mathf.Clamp(pageIndex, 0, totalPageCount - 1);

        if (_currentPageIndex == clampedPageIndex && _isBuilt)
        {
            RefreshNavigationState();
            return;
        }

        _currentPageIndex = clampedPageIndex;
        Rebuild();
    }

    public void GoToFirstPage()
    {
        GoToPage(0);
    }

    private void HandleDiscovered(string itemId)
    {
        if (_logEnabled)
        {
            Debug.Log($"[UIPictorialBookPage] OnDiscovered itemId={itemId}, currentCategory={_category}, currentPage={CurrentPageNumber}");
        }

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

    private bool CanMovePrevPage()
    {
        return _currentPageIndex > 0;
    }

    private bool CanMoveNextPage()
    {
        int totalPageCount = GetTotalPageCount(_cachedEntries != null ? _cachedEntries.Count : 0);
        return _currentPageIndex < totalPageCount - 1;
    }

    private void RefreshNavigationState()
    {
        int totalPageCount = GetTotalPageCount(_cachedEntries != null ? _cachedEntries.Count : 0);

        bool canMovePrev = _currentPageIndex > 0;
        bool canMoveNext = totalPageCount > 0 && _currentPageIndex < totalPageCount - 1;

        if (_prevPageButton != null)
        {
            _prevPageButton.interactable = canMovePrev;
        }

        if (_nextPageButton != null)
        {
            _nextPageButton.interactable = canMoveNext;
        }

        if (_pageIndexText != null)
        {
            _pageIndexText.text = string.Format(_pageTextFormat, _currentPageIndex + 1, totalPageCount);
        }
    }

    private int GetPageCapacity()
    {
        return Mathf.Max(0, _leftPageCapacity) + Mathf.Max(0, _rightPageCapacity);
    }

    private int GetTotalPageCount(int totalEntryCount)
    {
        int pageCapacity = GetPageCapacity();
        if (pageCapacity <= 0)
        {
            return 0;
        }

        // 엔트리가 0개여도 빈 슬롯 페이지 1장은 유지
        if (totalEntryCount <= 0)
        {
            return 1;
        }

        return Mathf.CeilToInt((float)totalEntryCount / pageCapacity);
    }

    private Transform GetParentByIndex(int localIndex, int leftCapacity, int rightCapacity)
    {
        if (_leftSlotRoot != null && localIndex < leftCapacity)
        {
            return _leftSlotRoot;
        }

        if (_rightSlotRoot != null && localIndex >= leftCapacity && localIndex < leftCapacity + rightCapacity)
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
