using UnityEngine;

/// <summary>
/// 프로그램 창 크기의 변화를 인식하여 부착된 카메라의 Orthographic Size를 갱신합니다.
/// </summary>
public class CameraAspectUpdater : Frameable
{
    private Camera _cam;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        if (_cam == null)
        {
            return;
        }
        // 혹시 게임 매니저가 없을 경우 대비
        ScreenData screen = DataManager.Ins.Screen;
        if (UDebug.IsNull(screen))
        {
            return;
        }
        // 프로그램 창 크기가 달라질 경우
        if (screen.Width != Screen.width || screen.Height != Screen.height)
        {
            screen.Width = Screen.width;
            screen.Height = Screen.height;
            UCamera.SetCameraAspect(_cam, screen.Width, screen.Height);
            OnResolutionChanged.Publish(screen.Width, screen.Height);
            UDebug.Print("프로그램 창 크기 변경이 감지되었습니다.");
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void CameraCaching()
    {
        _cam = UObject.GetComponent<Camera>(this.gameObject);
        // 혹시 게임 매니저가 없을 경우 대비
        ScreenData screen = DataManager.Ins.Screen;
        if (UDebug.IsNull(screen))
        {
            return;
        }
        screen.Height = Screen.height;
        screen.Width = Screen.width;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        CameraCaching();
    }
    #endregion
}
