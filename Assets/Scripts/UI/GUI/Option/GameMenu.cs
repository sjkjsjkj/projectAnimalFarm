using UnityEngine;

/// <summary>
/// 게임 메뉴 Ui
/// </summary>
public class GameMenu : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private GameMenuButton _menuWindow;
    [SerializeField] private GameMenuScreen _resolutionWindow;
    [SerializeField] private GameMenuAudio _audioWindow;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void MenuEnableHandle()
    {
        UObject.SetActive(_menuWindow.gameObject, true);
    }

    private void SceneLoadStartHandle(OnSceneLoadStart ctx)
    {
        EscManager.Ins.Exit(_menuWindow);
        UObject.SetActive(_menuWindow.gameObject, false);
        EscManager.Ins.Exit(_resolutionWindow);
        UObject.SetActive(_resolutionWindow.gameObject, false);
        EscManager.Ins.Exit(_audioWindow);
        UObject.SetActive(_audioWindow.gameObject, false);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Start()
    {
        EscManager.Ins.OnEnableEscPressed += MenuEnableHandle;
    }

    private void OnEnable()
    {
        EventBus<OnSceneLoadStart>.Subscribe(SceneLoadStartHandle);
    }

    private void OnDisable()
    {
        EventBus<OnSceneLoadStart>.Unsubscribe(SceneLoadStartHandle);
    }
    #endregion
}
