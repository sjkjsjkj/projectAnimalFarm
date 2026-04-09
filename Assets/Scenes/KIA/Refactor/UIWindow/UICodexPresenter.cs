using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 도감 UI 창의 열기/닫기와 공통 UI 창 규칙을 담당하는 프리젠터입니다.
/// 상세 패널이 열려 있을 경우 Esc 요청 시 상세 패널을 먼저 닫습니다.
/// </summary>
public class UICodexPresenter : BaseMono, IUIWindow
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    /// <summary>
    /// 실제로 열고 닫을 도감 창 루트 오브젝트입니다.
    /// </summary>
    [Header("도감 창 참조")]
    [SerializeField] private GameObject _windowRoot;

    /// <summary>
    /// 닫기 버튼 참조입니다.
    /// </summary>
    [SerializeField] private Button _closeButton;

    /// <summary>
    /// 도감 상세 패널 참조입니다.
    /// </summary>
    [SerializeField] private UIPictorialBookDetailPanel _detailPanel;

    /// <summary>
    /// Esc 입력으로 닫을 수 있는지 여부입니다.
    /// </summary>
    [SerializeField] private bool _canCloseWithEsc = true;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    /// <summary>
    /// 현재 창이 열려 있는지 여부입니다.
    /// </summary>
    public bool IsOpen
    {
        get
        {
            if (_windowRoot == null)
            {
                return false;
            }

            return _windowRoot.activeSelf;
        }
    }

    /// <summary>
    /// 현재 창이 Esc로 닫힐 수 있는지 여부입니다.
    /// </summary>
    public bool CanCloseWithEsc => _canCloseWithEsc;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 닫기 버튼 클릭을 처리합니다.
    /// </summary>
    private void CloseButtonClickedHandle()
    {
        Close();
    }

    /// <summary>
    /// 상세 패널이 열려 있으면 먼저 닫습니다.
    /// </summary>
    /// <returns>
    /// 상세 패널을 닫았으면 true, 닫을 상세 패널이 없으면 false
    /// </returns>
    private bool TryCloseDetailPanelFirst()
    {
        if (_detailPanel == null)
        {
            return false;
        }

        if (_detailPanel.IsShowing() == false)
        {
            return false;
        }

        _detailPanel.Hide();
        return true;
    }

    /// <summary>
    /// UIWindowStackManager를 안전하게 가져옵니다.
    /// </summary>
    private UIWindowStackManager GetStackManager()
    {
        return UIWindowStackManager.Ins;
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 도감 창을 엽니다.
    /// 이미 열려 있는 경우 스택 재등록만 수행합니다.
    /// </summary>
    public void Open()
    {
        if (_windowRoot == null)
        {
            return;
        }

        if (_windowRoot.activeSelf == false)
        {
            _windowRoot.SetActive(true);
        }

        UIWindowStackManager manager = GetStackManager();
        if (manager != null)
        {
            manager.RegisterWindow(this);
        }
    }

    /// <summary>
    /// 도감 창을 닫습니다.
    /// 상세 패널이 열려 있으면 상세 패널을 먼저 닫고,
    /// 상세 패널이 닫혀 있으면 도감 창 자체를 닫습니다.
    /// </summary>
    public void Close()
    {
        // 상세 패널이 열려 있으면 먼저 닫고 종료
        if (TryCloseDetailPanelFirst() == true)
        {
            return;
        }

        UIWindowStackManager manager = GetStackManager();
        if (manager != null)
        {
            manager.UnregisterWindow(this);
        }

        if (_windowRoot == null)
        {
            return;
        }

        if (_windowRoot.activeSelf == true)
        {
            _windowRoot.SetActive(false);
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(CloseButtonClickedHandle);
        }

        // 방어:
        // Open()을 통하지 않고 SetActive(true)로 활성화된 경우에도
        // 현재 열린 창이라면 스택에 등록합니다.
        if (IsOpen == true)
        {
            UIWindowStackManager manager = GetStackManager();
            if (manager != null)
            {
                manager.RegisterWindow(this);
            }
        }
    }

    private void OnDisable()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(CloseButtonClickedHandle);
        }

        // 방어:
        // SetActive(false), 부모 비활성화, 씬 전환 등으로 꺼질 때도
        // 스택에 참조가 남지 않도록 제거합니다.
        UIWindowStackManager manager = GetStackManager();
        if (manager != null)
        {
            manager.UnregisterWindow(this);
        }
    }

    private void Reset()
    {
        if (_windowRoot == null)
        {
            _windowRoot = gameObject;
        }

        if (_closeButton == null)
        {
            Transform closeButton = transform.Find("Header/CloseButton");
            if (closeButton != null)
            {
                _closeButton = closeButton.GetComponent<Button>();
            }
        }

        if (_detailPanel == null)
        {
            _detailPanel = GetComponentInChildren<UIPictorialBookDetailPanel>(true);
        }
    }
    #endregion
}
