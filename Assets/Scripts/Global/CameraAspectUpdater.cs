using UnityEngine;

/// <summary>
/// 프로그램 창 크기의 변화를 인식하여 부착된 카메라의 Orthographic Size를 갱신합니다.
/// </summary>
public class CameraAspectUpdater : Frameable
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Camera _cam;
    private int _lastWidth;
    private int _lastHeight;
    #endregion

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
        // 프로그램 창 크기가 달라질 경우
        if (_lastWidth != Screen.width || _lastHeight != Screen.height)
        {
            UCamera.SetCameraAspect(_cam);
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void CameraCaching()
    {
        _cam = UObject.GetComponent<Camera>(this.gameObject);
        _lastHeight = Screen.height;
        _lastWidth = Screen.width;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        CameraCaching();
    }
    #endregion
}
