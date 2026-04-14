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
/// - 현재 선택된 탭만 선명하게 보이고
///   나머지 탭은 반투명하게 표시
/// - ESC는 EscManager를 통해 닫히도록 등록하여
///   도감을 닫을 때 옵션 메뉴가 함께 뜨지 않게 처리
/// </summary>
public class UIPictorialBookController : BaseMono, IEscClosable
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

    [Header("탭 버튼 루트 (전체 버튼 오브젝트 연결 권장)")]
    [SerializeField] private GameObject _animalTabRoot;
    [SerializeField] private GameObject _fishTabRoot;
    [SerializeField] private GameObject _gatherTabRoot;

    [Header("탭 아이콘 (선택)")]
    [SerializeField] private Image _animalTabIcon;
    [SerializeField] private Image _fishTabIcon;
    [SerializeField] private Image _gatherTabIcon;

    [Header("탭 색상")]
    [SerializeField] private Color _selectedTabColor = Color.white;
    [SerializeField] private Color _unselectedTabColor = new Color(1f, 1f, 1f, 0.55f);

    [Header("탭 알파")]
    [Tooltip("현재 선택된 탭은 이 알파값으로 표시")]
    [SerializeField] private float _selectedTabAlpha = 1f;

    [Tooltip("선택되지 않은 탭은 이 알파값으로 표시")]
    [SerializeField] private float _unselectedTabAlpha = 0.35f;

    [Header("자동 연결")]
    [Tooltip("탭 루트를 비워두면 아이콘의 부모 오브젝트를 자동으로 탭 루트로 사용 시도")]
    [SerializeField] private bool _autoBindTabRootFromIconParent = true;

    [Header("로그")]
    [SerializeField] private bool _logEnabled = true;

    private CanvasGroup _bookCanvasGroup;
    private CanvasGroup _animalTabCanvasGroup;
    private CanvasGroup _fishTabCanvasGroup;
    private CanvasGroup _gatherTabCanvasGroup;

    private bool _isInitialized = false;
    private bool _isEscRegistered = false;
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

        // ESC는 직접 닫지 않고 EscManager를 통해 닫히도록 맡깁니다.
        // 그렇지 않으면 같은 프레임에 옵션 메뉴가 함께 열릴 수 있습니다.
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
        RegisterEscClose();

        if (_logEnabled)
        {
            Debug.Log("[UIPictorialBookController] 도감 열기");
        }
    }

    public void CloseBook()
    {
        Initialize();
        SetBookVisible(false);
        UnregisterEscClose();

        if (_logEnabled)
        {
            Debug.Log("[UIPictorialBookController] 도감 닫기");
        }
    }

    public void CloseUi()
    {
        CloseBook();
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

        TryAutoBindTabRoots();
        _animalTabCanvasGroup = GetOrAddCanvasGroup(_animalTabRoot, _animalTabCanvasGroup);
        _fishTabCanvasGroup = GetOrAddCanvasGroup(_fishTabRoot, _fishTabCanvasGroup);
        _gatherTabCanvasGroup = GetOrAddCanvasGroup(_gatherTabRoot, _gatherTabCanvasGroup);

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

    private void RegisterEscClose()
    {
        if (_isEscRegistered)
        {
            return;
        }

        EscManager.Ins.Enter(this);
        _isEscRegistered = true;
    }

    private void UnregisterEscClose()
    {
        if (_isEscRegistered == false)
        {
            return;
        }

        EscManager.Ins.Exit(this);
        _isEscRegistered = false;
    }

    private void RefreshTabVisual(string category)
    {
        bool isAnimal = IsSameCategory(category, "Animal");
        bool isFish = IsSameCategory(category, "Fish");
        bool isGather = IsSameCategory(category, "Gather");

        ApplyTabVisual(_animalTabCanvasGroup, _animalTabIcon, isAnimal);
        ApplyTabVisual(_fishTabCanvasGroup, _fishTabIcon, isFish);
        ApplyTabVisual(_gatherTabCanvasGroup, _gatherTabIcon, isGather);
    }

    private void ApplyTabVisual(CanvasGroup canvasGroup, Image icon, bool isSelected)
    {
        float alpha = isSelected ? _selectedTabAlpha : _unselectedTabAlpha;
        Color color = isSelected ? _selectedTabColor : _unselectedTabColor;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Clamp01(alpha);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (icon != null)
        {
            icon.color = color;
        }
    }

    private void TryAutoBindTabRoots()
    {
        if (!_autoBindTabRootFromIconParent)
        {
            return;
        }

        if (_animalTabRoot == null && _animalTabIcon != null && _animalTabIcon.transform.parent != null)
        {
            _animalTabRoot = _animalTabIcon.transform.parent.gameObject;
        }

        if (_fishTabRoot == null && _fishTabIcon != null && _fishTabIcon.transform.parent != null)
        {
            _fishTabRoot = _fishTabIcon.transform.parent.gameObject;
        }

        if (_gatherTabRoot == null && _gatherTabIcon != null && _gatherTabIcon.transform.parent != null)
        {
            _gatherTabRoot = _gatherTabIcon.transform.parent.gameObject;
        }
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject target, CanvasGroup current)
    {
        if (target == null)
        {
            return current;
        }

        CanvasGroup canvasGroup = current != null ? current : target.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
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

    private void OnDisable()
    {
        UnregisterEscClose();
    }
}
