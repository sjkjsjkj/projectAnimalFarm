using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 도감 UI 전체 컨트롤러
///
/// 핵심
/// - O 키로 도감 열기 / 닫기
/// - 탭 전환
/// - 도감 열 때 / 탭 바꿀 때 강제 전체 갱신
/// </summary>
public class UIPictorialBookController : BaseMono
{
    [Header("도감 루트")]
    [SerializeField] private GameObject _bookRoot;

    [Header("도감 페이지")]
    [SerializeField] private UIPictorialBookPage _bookPage;

    [Header("기본 카테고리")]
    [SerializeField] private string _defaultCategory = "Animal";

    [Header("키 입력")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.O;
    [SerializeField] private KeyCode _closeKey = KeyCode.Escape;

    [Header("탭 아이콘 (선택)")]
    [SerializeField] private Image _animalTabIcon;
    [SerializeField] private Image _fishTabIcon;
    [SerializeField] private Image _gatherTabIcon;

    [Header("탭 색상")]
    [SerializeField] private Color _selectedTabColor = Color.white;
    [SerializeField] private Color _unselectedTabColor = new Color(1f, 1f, 1f, 0.55f);

    [Header("로그")]
    [SerializeField] private bool _logEnabled = true;

    private CanvasGroup _bookCanvasGroup;
    private bool _isInitialized = false;
    private string _currentCategory;

    public bool IsOpen
    {
        get
        {
            return _bookCanvasGroup != null &&
                   _bookCanvasGroup.alpha > 0.99f &&
                   _bookCanvasGroup.interactable;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Start()
    {
        Initialize();

        _currentCategory = string.IsNullOrWhiteSpace(_defaultCategory)
            ? "Animal"
            : _defaultCategory.Trim();

        RefreshTabVisual(_currentCategory);
        SetBookVisible(false);
    }

    private void Update()
    {
        if (ShouldIgnoreHotkey())
        {
            return;
        }

        if (Input.GetKeyDown(_toggleKey))
        {
            ToggleBook();
            return;
        }

        if (IsOpen && Input.GetKeyDown(_closeKey))
        {
            CloseBook();
        }
    }

    public void ToggleBook()
    {
        if (IsOpen)
        {
            CloseBook();
        }
        else
        {
            OpenBook();
        }
    }

    public void OpenBook()
    {
        Initialize();

        if (_bookRoot == null)
        {
            Debug.LogWarning("[UIPictorialBookController] BookRoot가 연결되지 않았습니다.");
            return;
        }

        if (_bookPage != null)
        {
            _bookPage.SetCategory(_currentCategory, false);
            _bookPage.ForceFullRefresh();
        }

        RefreshTabVisual(_currentCategory);
        _bookRoot.transform.SetAsLastSibling();
        SetBookVisible(true);

        if (_logEnabled)
        {
            Debug.Log("[UIPictorialBookController] 도감 열기");
        }
    }

    public void CloseBook()
    {
        Initialize();
        SetBookVisible(false);

        if (_logEnabled)
        {
            Debug.Log("[UIPictorialBookController] 도감 닫기");
        }
    }

    public void OnClickAnimalTab()
    {
        ShowCategory("Animal");
    }

    public void OnClickFishTab()
    {
        ShowCategory("Fish");
    }

    public void OnClickGatherTab()
    {
        ShowCategory("Gather");
    }

    public void ShowCategory(string category)
    {
        Initialize();

        string normalizedCategory = NormalizeCategory(category);
        if (string.IsNullOrWhiteSpace(normalizedCategory))
        {
            Debug.LogWarning("[UIPictorialBookController] category가 비어 있습니다.");
            return;
        }

        _currentCategory = normalizedCategory;

        if (_bookPage == null)
        {
            Debug.LogWarning("[UIPictorialBookController] UIPictorialBookPage가 연결되지 않았습니다.");
            return;
        }

        _bookPage.SetCategory(_currentCategory, false);
        _bookPage.ForceFullRefresh();
        RefreshTabVisual(_currentCategory);

        if (_logEnabled)
        {
            Debug.Log($"[UIPictorialBookController] 도감 카테고리 변경: {_currentCategory}");
        }
    }

    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        if (_bookRoot != null)
        {
            _bookCanvasGroup = _bookRoot.GetComponent<CanvasGroup>();

            if (_bookCanvasGroup == null)
            {
                _bookCanvasGroup = _bookRoot.AddComponent<CanvasGroup>();
            }
        }

        _isInitialized = true;
    }

    private void SetBookVisible(bool isVisible)
    {
        if (_bookCanvasGroup == null)
        {
            return;
        }

        _bookCanvasGroup.alpha = isVisible ? 1f : 0f;
        _bookCanvasGroup.interactable = isVisible;
        _bookCanvasGroup.blocksRaycasts = isVisible;
    }

    private void RefreshTabVisual(string category)
    {
        bool isAnimal = IsSameCategory(category, "Animal");
        bool isFish = IsSameCategory(category, "Fish");
        bool isGather = IsSameCategory(category, "Gather");

        if (_animalTabIcon != null)
        {
            _animalTabIcon.color = isAnimal ? _selectedTabColor : _unselectedTabColor;
        }

        if (_fishTabIcon != null)
        {
            _fishTabIcon.color = isFish ? _selectedTabColor : _unselectedTabColor;
        }

        if (_gatherTabIcon != null)
        {
            _gatherTabIcon.color = isGather ? _selectedTabColor : _unselectedTabColor;
        }
    }

    private bool IsSameCategory(string a, string b)
    {
        return string.Equals(
            NormalizeCategory(a),
            NormalizeCategory(b),
            StringComparison.OrdinalIgnoreCase);
    }

    private string NormalizeCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return string.Empty;
        }

        return category.Trim();
    }

    private bool ShouldIgnoreHotkey()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject == null)
        {
            return false;
        }

        if (selectedObject.GetComponent<TMP_InputField>() != null)
        {
            return true;
        }

        if (selectedObject.GetComponent<InputField>() != null)
        {
            return true;
        }

        return false;
    }
}
