using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 UI 창의 열기/닫기와 ESC 닫기를 담당하는 프리젠터입니다.
/// 현재 구조에서는 EscManager가 ESC 입력과 닫기 우선순위를 담당합니다.
/// </summary>
public class UISettingsPresenter : BaseMono, IEscClosable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    /// <summary>
    /// 실제로 열고 닫을 설정 창 루트 오브젝트입니다.
    /// </summary>
    [Header("설정 창 참조")]
    [SerializeField] private GameObject _windowRoot;

    /// <summary>
    /// 닫기 버튼 참조입니다.
    /// </summary>
    [SerializeField] private Button _closeButton;
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
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 닫기 버튼 클릭을 처리합니다.
    /// </summary>
    private void CloseButtonClickedHandle()
    {
        Close();
    }

    /*
    /// <summary>
    /// 예전 UIWindowStackManager 스택 방식에서 사용하던 함수입니다.
    /// 현재 구조에서는 EscManager가 ESC를 담당하므로 사용하지 않습니다.
    /// </summary>
    private UIWindowStackManager GetStackManager()
    {
        return UIWindowStackManager.Ins;
    }
    */
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 설정 창을 엽니다.
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
    }

    /// <summary>
    /// 설정 창을 닫습니다.
    /// </summary>
    public void Close()
    {
        if (_windowRoot == null)
        {
            return;
        }

        if (_windowRoot.activeSelf == true)
        {
            _windowRoot.SetActive(false);
        }
    }

    /// <summary>
    /// ESC가 눌렸을 때 EscManager가 호출하는 닫기 함수입니다.
    /// </summary>
    public void CloseUi()
    {
        Close();
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

        // 현재 설정창이 열려 있으면 EscManager에 등록합니다.
        if (IsOpen == true)
        {
            EscManager.Ins.Enter(this);
        }
    }

    private void OnDisable()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(CloseButtonClickedHandle);
        }

        EscManager.Ins.Exit(this);
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
    }
    #endregion
}
