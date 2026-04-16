using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 메뉴 Ui
/// Dim은 GameMenu가 열려 있을 때만 사용합니다.
/// </summary>
public class GameMenu : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private GameMenuButton _menuWindow;
    [SerializeField] private GameMenuScreen _resolutionWindow;
    [SerializeField] private GameMenuAudio _audioWindow;
    [SerializeField] private Button _dimButton;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public void OpenMenu()
    {
        if (_audioWindow != null)
        {
            EscManager.Ins.Exit(_audioWindow);
            UObject.SetActive(_audioWindow.gameObject, false);
        }

        if (_resolutionWindow != null)
        {
            EscManager.Ins.Exit(_resolutionWindow);
            UObject.SetActive(_resolutionWindow.gameObject, false);
        }

        if (_menuWindow != null)
        {
            UObject.SetActive(_menuWindow.gameObject, true);
        }

        RefreshDimState();
    }

    /// <summary>
    /// 해상도 옵션 창을 엽니다.
    /// GameMenu는 닫고, Dim도 끕니다.
    /// </summary>
    public void OpenResolutionWindow()
    {
        if (_menuWindow != null)
        {
            EscManager.Ins.Exit(_menuWindow);
            UObject.SetActive(_menuWindow.gameObject, false);
        }

        if (_audioWindow != null)
        {
            EscManager.Ins.Exit(_audioWindow);
            UObject.SetActive(_audioWindow.gameObject, false);
        }

        if (_resolutionWindow != null)
        {
            UObject.SetActive(_resolutionWindow.gameObject, true);
        }

        RefreshDimState();
    }

    /// <summary>
    /// 오디오 옵션 창을 엽니다.
    /// GameMenu는 닫고, Dim도 끕니다.
    /// </summary>
    public void OpenAudioWindow()
    {
        if (_menuWindow != null)
        {
            EscManager.Ins.Exit(_menuWindow);
            UObject.SetActive(_menuWindow.gameObject, false);
        }

        if (_resolutionWindow != null)
        {
            EscManager.Ins.Exit(_resolutionWindow);
            UObject.SetActive(_resolutionWindow.gameObject, false);
        }

        if (_audioWindow != null)
        {
            UObject.SetActive(_audioWindow.gameObject, true);
        }

        RefreshDimState();
    }

    /// <summary>
    /// 메뉴 관련 창을 모두 닫습니다.
    /// </summary>
    public void CloseAllWindows()
    {
        if (_menuWindow != null)
        {
            EscManager.Ins.Exit(_menuWindow);
            UObject.SetActive(_menuWindow.gameObject, false);
        }

        if (_resolutionWindow != null)
        {
            EscManager.Ins.Exit(_resolutionWindow);
            UObject.SetActive(_resolutionWindow.gameObject, false);
        }

        if (_audioWindow != null)
        {
            EscManager.Ins.Exit(_audioWindow);
            UObject.SetActive(_audioWindow.gameObject, false);
        }

        RefreshDimState();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 딤 버튼 이벤트를 연결합니다.
    /// </summary>
    private void BindButton()
    {
        if (_dimButton == null)
        {
            return;
        }

        _dimButton.onClick.RemoveListener(HandleClickDim);
        _dimButton.onClick.AddListener(HandleClickDim);
    }

    private void MenuEnableHandle()
    {
        //UObject.SetActive(_menuWindow.gameObject, true);
        OpenMenu();
    }

    /// <summary>
    /// 딤 클릭 시 메뉴 관련 창을 모두 닫습니다.
    /// </summary>
    private void HandleClickDim()
    {
        CloseAllWindows();
    }

    /// <summary>
    /// 게임 메뉴가 열려 있는지 기준으로 딤 표시 상태를 갱신합니다.
    /// </summary>
    private void RefreshDimState()
    {
        if (_dimButton == null)
        {
            return;
        }

        bool isMenuOpen = _menuWindow != null && _menuWindow.gameObject.activeSelf == true;
        UObject.SetActive(_dimButton.gameObject, isMenuOpen);
    }

    /// <summary>
    /// 씬 로드 시작 시 메뉴 관련 창을 모두 닫습니다.
    /// </summary>
    private void SceneLoadStartHandle(OnSceneLoadStart ctx)
    {
        CloseAllWindows();
    }

    //private void SceneLoadStartHandle(OnSceneLoadStart ctx)
    //{
    //    EscManager.Ins.Exit(_menuWindow);
    //    UObject.SetActive(_menuWindow.gameObject, false);
    //    EscManager.Ins.Exit(_resolutionWindow);
    //    UObject.SetActive(_resolutionWindow.gameObject, false);
    //    EscManager.Ins.Exit(_audioWindow);
    //    UObject.SetActive(_audioWindow.gameObject, false);
    //}
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        BindButton();
    }

    private void Start()
    {
        EscManager.Ins.OnEnableEscPressed += MenuEnableHandle;
        RefreshDimState();
    }

    private void OnEnable()
    {
        EventBus<OnSceneLoadStart>.Subscribe(SceneLoadStartHandle);
    }

    private void OnDisable()
    {
        EventBus<OnSceneLoadStart>.Unsubscribe(SceneLoadStartHandle);

        if (EscManager.Ins != null)
        {
            EscManager.Ins.OnEnableEscPressed -= MenuEnableHandle;
        }

        if (_dimButton != null)
        {
            _dimButton.onClick.RemoveListener(HandleClickDim);
        }
    }
    #endregion
}
