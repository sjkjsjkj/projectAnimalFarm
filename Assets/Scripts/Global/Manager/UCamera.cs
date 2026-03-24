using UnityEngine;

/// <summary>
/// 카메라를 캐싱하고 기능을 지원하는 유틸리티입니다.
/// </summary>
public static class UCamera
{
    private static Camera _mainCamera;
    private static Camera _uiCamera;

    /// <summary>
    /// 현재 씬에서 메인 카메라를 탐색해서 반환합니다.
    /// </summary>
    public static Camera MainCamera
    {
        get
        {
            if (_mainCamera == null)
            {
                _mainCamera = UObject.FindComponent<Camera>(K.NAME_MAIN_CAMERA);
                if(_mainCamera == null)
                {
                    UDebug.PrintOnce($"메인 카메라를 찾을 수 없습니다.", LogType.Assert);
                    return null;
                }
                SetCameraAspect(_mainCamera); // 카메라를 새로 찾을 때마다 조절
            }
            return _mainCamera;
        }
    }

    /// <summary>
    /// 현재 씬에서 UI 카메라를 탐색해서 반환합니다.
    /// </summary>
    public static Camera UICamera
    {
        get
        {
            if (_uiCamera == null)
            {
                _uiCamera = UObject.FindComponent<Camera>("UI Camera");
                if (_uiCamera == null)
                {
                    UDebug.PrintOnce($"UI 카메라를 찾을 수 없습니다.", LogType.Assert);
                    return null;
                }
                // UI 카메라는 Canvas Scaler에게 의존한다.
            }
            return _uiCamera;
        }
    }

    /// <summary>
    /// 기기 비율에 맞춰 카메라의 Orthographic Size를 늘립니다.
    /// </summary>
    /// <param name="cam">카메라 오브젝트</param>
    private static void SetCameraAspect(Camera cam)
    {
        if (cam == null)
        {
            return;
        }
        cam.orthographic = true; // 직교 카메라
        cam.orthographicSize = K.CAMERA_MIN_HEIGHT / 2f; // 최소 크기
        // 현재 기기의 화면 비율
        float curAspect = (float)Screen.width / (float)Screen.height; // 값이 클수록 가로가 길다
        if (curAspect < K.CAMERA_ASPECT) // 가로가 좁아서 양쪽에 있는 오브젝트들이 보이지 않을 경우
        {
            cam.orthographicSize *= (K.CAMERA_ASPECT / curAspect); // 카메라를 그만큼 늘린다.
        }
    }
}
