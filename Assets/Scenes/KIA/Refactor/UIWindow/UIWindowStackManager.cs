using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 열린 UI 창 스택을 관리하고, 뒤로가기 요청 시 최상단 창을 닫아주는 매니저입니다.
/// 닫을 창이 없을 경우 fallback 창(예: 설정 창)을 열 수 있습니다.
/// </summary>
public class UIWindowStackManager : Singleton<UIWindowStackManager>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    /// <summary>
    /// 닫을 창이 아무것도 없을 때 대신 열어줄 fallback 창입니다.
    /// 일반적으로 SettingsWindow를 연결합니다.
    /// </summary>
    [Header("공통 UI 창 관리 참조")]
    [SerializeField] private BaseMono _fallbackWindowMono;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    /// <summary>
    /// 현재 열린 UI 창 스택입니다.
    /// </summary>
    private readonly List<IUIWindow> _windowStack = new List<IUIWindow>();

    /// <summary>
    /// fallback 창 인터페이스 참조입니다.
    /// </summary>
    private IUIWindow _fallbackWindow;

    /// <summary>
    /// 초기화 중복 방지 플래그입니다.
    /// </summary>
    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// fallback 창 인터페이스를 연결합니다.
    /// 실패 시 이전 참조가 남지 않도록 null로 초기화합니다.
    /// </summary>
    private void ResolveFallbackWindow()
    {
        _fallbackWindow = null;

        if (_fallbackWindowMono == null)
        {
            return;
        }

        if (_fallbackWindowMono.TryGetComponent(out _fallbackWindow) == false)
        {
            _fallbackWindow = null;
            UDebug.Print("UIWindowStackManager에 연결된 fallback 오브젝트가 IUIWindow를 구현하지 않았습니다.", LogType.Assert);
        }
    }

    /// <summary>
    /// IUIWindow 참조가 실제로 null인지 안전하게 검사합니다.
    /// interface 타입으로 참조된 MonoBehaviour는 Unity의 == null 오버라이딩이
    /// 동작하지 않을 수 있으므로 UnityEngine.Object로 캐스팅 후 검사합니다.
    /// </summary>
    /// <param name="window">검사할 UI 창</param>
    private static bool IsNullWindow(IUIWindow window)
    {
        if (window == null)
        {
            return true;
        }

        if (window is UnityEngine.Object unityObject)
        {
            return unityObject == null;
        }

        return false;
    }

    /// <summary>
    /// 스택에서 null이거나 닫혀 있는 비정상 참조를 정리합니다.
    /// </summary>
    private void CleanupInvalidWindows()
    {
        for (int i = _windowStack.Count - 1; i >= 0; --i)
        {
            IUIWindow window = _windowStack[i];

            if (IsNullWindow(window) == true)
            {
                _windowStack.RemoveAt(i);
                continue;
            }

            if (window.IsOpen == false)
            {
                _windowStack.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 최상단 창을 닫을 수 있으면 닫습니다.
    /// top 창이 Esc로 닫힐 수 없는 경우, 아래 창까지 내려가지 않고 종료합니다.
    /// </summary>
    private bool TryCloseTopWindow()
    {
        CleanupInvalidWindows();

        if (_windowStack.Count == 0)
        {
            return false;
        }

        int topIndex = _windowStack.Count - 1;
        IUIWindow topWindow = _windowStack[topIndex];

        if (IsNullWindow(topWindow) == true)
        {
            _windowStack.RemoveAt(topIndex);
            return false;
        }

        if (topWindow.IsOpen == false)
        {
            _windowStack.RemoveAt(topIndex);
            return false;
        }

        // top 창이 Esc로 닫히지 않는 정책이면 더 아래 창은 건드리지 않습니다.
        if (topWindow.CanCloseWithEsc == false)
        {
            return true;
        }

        topWindow.Close();
        return true;
    }

    /// <summary>
    /// 스택이 비어 있을 때 fallback 창을 엽니다.
    /// </summary>
    private bool TryOpenFallbackWindow()
    {
        if (IsNullWindow(_fallbackWindow) == true)
        {
            return false;
        }

        if (_fallbackWindow.IsOpen == true)
        {
            return false;
        }

        _fallbackWindow.Open();
        return true;
    }

    /// <summary>
    /// 뒤로가기 요청 이벤트를 처리합니다.
    /// </summary>
    /// <param name="channel">뒤로가기 요청 이벤트</param>
    private void UIBackRequestedHandle(OnUIBackRequested channel)
    {
        if (TryCloseTopWindow() == true)
        {
            return;
        }

        TryOpenFallbackWindow();
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 싱글톤 초기화를 수행합니다.
    /// </summary>
    public override void Initialize()
    {
        if (_isInitialized == true)
        {
            return;
        }

        ResolveFallbackWindow();
        _isInitialized = true;
    }

    /// <summary>
    /// 열린 UI 창을 스택에 등록합니다.
    /// 이미 등록된 창은 최상단으로 재배치합니다.
    /// </summary>
    /// <param name="window">등록할 UI 창</param>
    public void RegisterWindow(IUIWindow window)
    {
        if (IsNullWindow(window) == true)
        {
            return;
        }

        if (window.IsOpen == false)
        {
            return;
        }

        CleanupInvalidWindows();

        if (_windowStack.Contains(window) == true)
        {
            _windowStack.Remove(window);
        }

        _windowStack.Add(window);
    }

    /// <summary>
    /// 닫힌 UI 창을 스택에서 제거합니다.
    /// </summary>
    /// <param name="window">제거할 UI 창</param>
    public void UnregisterWindow(IUIWindow window)
    {
        if (IsNullWindow(window) == true)
        {
            return;
        }

        _windowStack.Remove(window);
    }

    /// <summary>
    /// 스택에 등록된 모든 창 수를 반환합니다.
    /// 디버깅용으로 활용할 수 있습니다.
    /// </summary>
    public int GetStackCount()
    {
        CleanupInvalidWindows();
        return _windowStack.Count;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        // 방어: Singleton 베이스가 Initialize를 자동 호출하지 않는 경우를 대비합니다.
        Initialize();
    }

    private void OnEnable()
    {
        EventBus<OnUIBackRequested>.Subscribe(UIBackRequestedHandle);
    }

    private void OnDisable()
    {
        EventBus<OnUIBackRequested>.Unsubscribe(UIBackRequestedHandle);
    }
    #endregion
}
