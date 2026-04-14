using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 창 열기 / 닫기 / 토글 요청을 처리하는 매니저입니다.
/// 
/// 현재 구조에서는 EscManager가 ESC 입력과 최상단 UI 닫기를 담당하므로,
/// UIWindowStackManager는 IUIWindow registry를 이용한 창 열기 / 닫기 / 토글만 담당합니다.
/// </summary>
public class UIWindowStackManager : Singleton<UIWindowStackManager>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("공통 UI 창 관리 참조")]
    //[SerializeField] private BaseMono _fallbackWindowMono;

    [Header("UI 창 레지스트리")]
    [SerializeField] private List<CWindowRegistryEntry> _windowRegistryEntries = new();
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    /// <summary>
    /// 창 식별자와 실제 창 컴포넌트를 연결하는 registry 엔트리입니다.
    /// </summary>
    [System.Serializable]
    private class CWindowRegistryEntry
    {
        [Header("창 식별자")]
        [SerializeField] private EUIWindowId _windowId = EUIWindowId.None;

        [Header("IUIWindow 구현 오브젝트")]
        [SerializeField] private BaseMono _windowMono;

        public EUIWindowId WindowId => _windowId;
        public BaseMono WindowMono => _windowMono;
    }
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    /// <summary>
    /// 비활성 시작 창까지 포함하는 전체 UI 창 registry입니다.
    /// </summary>
    private readonly Dictionary<EUIWindowId, IUIWindow> _windowRegistry = new();

    /// <summary>
    /// Initialize 중복 호출 방지 플래그입니다.
    /// </summary>
    private bool _isInitialized = false;

    /*
    /// <summary>
    /// 현재 열린 UI 창 스택입니다.
    /// EscManager를 유지하는 현재 구조에서는 사용하지 않습니다.
    /// </summary>
    private readonly List<IUIWindow> _windowStack = new();

    /// <summary>
    /// 스택이 비어 있을 때 열 수 있는 fallback 창입니다.
    /// 현재 구조에서는 EscManager가 담당하므로 사용하지 않습니다.
    /// </summary>
    private IUIWindow _fallbackWindow;
    */
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /*
    /// <summary>
    /// fallback 창 인터페이스를 연결합니다.
    /// 현재 구조에서는 EscManager가 fallback 처리하므로 사용하지 않습니다.
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
            UDebug.Print(
                "UIWindowStackManager에 연결된 fallback 오브젝트가 IUIWindow를 구현하지 않았습니다.",
                LogType.Assert);
        }
    }
    */

    /// <summary>
    /// inspector에 연결된 UI 창 registry를 해석합니다.
    /// 비활성 시작 창도 여기서 참조를 확보합니다.
    /// </summary>
    private void ResolveWindowRegistry()
    {
        _windowRegistry.Clear();

        int entryCount = _windowRegistryEntries.Count;
        for (int i = 0; i < entryCount; ++i)
        {
            CWindowRegistryEntry entry = _windowRegistryEntries[i];
            if (entry == null)
            {
                continue;
            }

            if (entry.WindowId == EUIWindowId.None)
            {
                UDebug.Print(
                    $"UIWindowStackManager registry[{i}]의 windowId가 None입니다.",
                    LogType.Warning);
                continue;
            }

            if (entry.WindowMono == null)
            {
                UDebug.Print(
                    $"UIWindowStackManager registry[{i}]의 windowMono 참조가 비어 있습니다.",
                    LogType.Warning);
                continue;
            }

            if (entry.WindowMono.TryGetComponent(out IUIWindow window) == false)
            {
                UDebug.Print(
                    $"UIWindowStackManager registry[{i}] 오브젝트가 IUIWindow를 구현하지 않았습니다.",
                    LogType.Assert);
                continue;
            }

            if (_windowRegistry.ContainsKey(entry.WindowId))
            {
                UDebug.Print(
                    $"UIWindowStackManager registry에 중복된 windowId가 있습니다. ({entry.WindowId})",
                    LogType.Assert);
                continue;
            }

            _windowRegistry.Add(entry.WindowId, window);
        }
    }

    /// <summary>
    /// interface 타입으로 참조된 MonoBehaviour는 Unity의 == null 오버라이딩이
    /// 기대와 다르게 동작할 수 있으므로 UnityEngine.Object로 캐스팅 후 검사합니다.
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
    /// registry에서 특정 창을 안전하게 가져옵니다.
    /// 참조가 무효화된 경우 registry를 한 번 재해석합니다.
    /// </summary>
    /// <param name="windowId">찾을 창 식별자</param>
    /// <param name="window">찾은 창</param>
    /// <returns>찾기 성공 여부</returns>
    private bool TryGetRegisteredWindow(EUIWindowId windowId, out IUIWindow window)
    {
        window = null;

        if (windowId == EUIWindowId.None)
        {
            return false;
        }

        if (_windowRegistry.TryGetValue(windowId, out window) && IsNullWindow(window) == false)
        {
            return true;
        }

        // 참조가 무효화된 경우 registry를 다시 해석한 뒤 한 번 더 시도합니다.
        ResolveWindowRegistry();

        if (_windowRegistry.TryGetValue(windowId, out window) && IsNullWindow(window) == false)
        {
            return true;
        }

        window = null;
        return false;
    }

    /*
    /// <summary>
    /// 스택에서 null이거나 이미 닫힌 비정상 참조를 정리합니다.
    /// 현재 구조에서는 사용하지 않습니다.
    /// </summary>
    private void CleanupInvalidWindows()
    {
        for (int i = _windowStack.Count - 1; i >= 0; --i)
        {
            IUIWindow window = _windowStack[i];

            if (IsNullWindow(window) == true || window.IsOpen == false)
            {
                _windowStack.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 최상단 창을 닫을 수 있으면 닫습니다.
    /// 현재 구조에서는 EscManager가 담당하므로 사용하지 않습니다.
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

        if (IsNullWindow(topWindow) == true || topWindow.IsOpen == false)
        {
            _windowStack.RemoveAt(topIndex);
            return false;
        }

        if (topWindow.CanCloseWithEsc == false)
        {
            return true;
        }

        topWindow.Close();
        return true;
    }

    /// <summary>
    /// 뒤로가기 요청 이벤트를 처리합니다.
    /// 현재 구조에서는 EscManager가 담당하므로 사용하지 않습니다.
    /// </summary>
    private void HandleUIBackRequested(OnUIBackRequested channel)
    {
        if (TryCloseTopWindow() == true)
        {
            return;
        }

        TryOpenFallbackWindow();
    }

    /// <summary>
    /// 스택이 비어 있을 때 fallback 창을 엽니다.
    /// 현재 구조에서는 EscManager가 담당하므로 사용하지 않습니다.
    /// </summary>
    private bool TryOpenFallbackWindow()
    {
        if (IsNullWindow(_fallbackWindow) == true || _fallbackWindow.IsOpen == true)
        {
            return false;
        }

        _fallbackWindow.Open();
        return _fallbackWindow.IsOpen;
    }
    */

    /// <summary>
    /// UI 창 열기 요청 이벤트를 처리합니다.
    /// </summary>
    /// <param name="channel">창 열기 요청 이벤트</param>
    private void HandleUIWindowOpenRequested(OnUIWindowOpenRequested channel)
    {
        TryOpenWindow(channel.windowId);
    }

    /// <summary>
    /// UI 창 닫기 요청 이벤트를 처리합니다.
    /// </summary>
    /// <param name="channel">창 닫기 요청 이벤트</param>
    private void HandleUIWindowCloseRequested(OnUIWindowCloseRequested channel)
    {
        TryCloseWindow(channel.windowId);
    }

    /// <summary>
    /// UI 창 토글 요청 이벤트를 처리합니다.
    /// </summary>
    /// <param name="channel">창 토글 요청 이벤트</param>
    private void HandleUIWindowToggleRequested(OnUIWindowToggleRequested channel)
    {
        TryToggleWindow(channel.windowId);
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

        // ResolveFallbackWindow();
        ResolveWindowRegistry();
        _isInitialized = true;
    }

    /// <summary>
    /// 특정 창이 현재 열려 있는지 반환합니다.
    /// </summary>
    /// <param name="windowId">검사할 창 식별자</param>
    /// <returns>열림 여부</returns>
    public bool IsWindowOpen(EUIWindowId windowId)
    {
        if (TryGetRegisteredWindow(windowId, out IUIWindow window) == false)
        {
            return false;
        }

        return window.IsOpen;
    }

    /// <summary>
    /// registry에 등록된 창을 엽니다.
    /// 비활성 시작 창도 참조만 있으면 열 수 있습니다.
    /// </summary>
    /// <param name="windowId">열 창 식별자</param>
    /// <returns>성공 여부</returns>
    public bool TryOpenWindow(EUIWindowId windowId)
    {
        if (TryGetRegisteredWindow(windowId, out IUIWindow window) == false)
        {
            UDebug.Print(
                $"UIWindowStackManager에 등록되지 않은 창 열기 요청입니다. ({windowId})",
                LogType.Warning);
            return false;
        }

        if (window.IsOpen == true)
        {
            return true;
        }

        window.Open();
        return window.IsOpen;
    }

    /// <summary>
    /// registry에 등록된 창을 닫습니다.
    /// </summary>
    /// <param name="windowId">닫을 창 식별자</param>
    /// <returns>성공 여부</returns>
    public bool TryCloseWindow(EUIWindowId windowId)
    {
        if (TryGetRegisteredWindow(windowId, out IUIWindow window) == false)
        {
            UDebug.Print(
                $"UIWindowStackManager에 등록되지 않은 창 닫기 요청입니다. ({windowId})",
                LogType.Warning);
            return false;
        }

        if (window.IsOpen == false)
        {
            return true;
        }

        window.Close();
        return true;
    }

    /// <summary>
    /// registry에 등록된 창을 토글합니다.
    /// </summary>
    /// <param name="windowId">토글할 창 식별자</param>
    /// <returns>성공 여부</returns>
    public bool TryToggleWindow(EUIWindowId windowId)
    {
        return IsWindowOpen(windowId) ? TryCloseWindow(windowId) : TryOpenWindow(windowId);
    }

    /*
    /// <summary>
    /// 열린 UI 창을 스택에 등록합니다.
    /// 현재 구조에서는 사용하지 않습니다.
    /// </summary>
    public void RegisterWindow(IUIWindow window)
    {
        if (IsNullWindow(window) == true || window.IsOpen == false)
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
    /// 현재 구조에서는 사용하지 않습니다.
    /// </summary>
    public void UnregisterWindow(IUIWindow window)
    {
        if (IsNullWindow(window) == true)
        {
            return;
        }

        _windowStack.Remove(window);
    }

    /// <summary>
    /// 스택에 등록된 유효한 창 수를 반환합니다.
    /// 현재 구조에서는 사용하지 않습니다.
    /// </summary>
    public int GetStackCount()
    {
        CleanupInvalidWindows();
        return _windowStack.Count;
    }
    */
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
    }

    private void OnEnable()
    {
        // EventBus<OnUIBackRequested>.Subscribe(HandleUIBackRequested);
        EventBus<OnUIWindowOpenRequested>.Subscribe(HandleUIWindowOpenRequested);
        EventBus<OnUIWindowCloseRequested>.Subscribe(HandleUIWindowCloseRequested);
        EventBus<OnUIWindowToggleRequested>.Subscribe(HandleUIWindowToggleRequested);
    }

    private void OnDisable()
    {
        // EventBus<OnUIBackRequested>.Unsubscribe(HandleUIBackRequested);
        EventBus<OnUIWindowOpenRequested>.Unsubscribe(HandleUIWindowOpenRequested);
        EventBus<OnUIWindowCloseRequested>.Unsubscribe(HandleUIWindowCloseRequested);
        EventBus<OnUIWindowToggleRequested>.Unsubscribe(HandleUIWindowToggleRequested);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _windowRegistry.Clear();

        /*
        _windowStack.Clear();
        */
    }
    #endregion
}
