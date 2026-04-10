using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 해상도 변경 로직을 제공합니다.
/// </summary>
public class GameMenuScreen : BaseMono, IEscClosable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private Toggle[] _resolutionField = new Toggle[4];
    [SerializeField] private Toggle _fullScreen;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly Vector2Int[] _resolutions =
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),
    };
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void CloseUi()
    {
        UObject.SetActive(gameObject, false);
    }
    public void CloseButton()
    {
        CloseUi();
        EscManager.Ins.Exit(this);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void ResolutionToggleHandle(int index)
    {
        int safe = Mathf.Clamp(index, 0, _resolutionField.Length - 1);
        int x = _resolutions[safe].x;
        int y = _resolutions[safe].y;
        Screen.SetResolution(x, y, Screen.fullScreen);
    }

    // 풀스크린 전환
    private void FullScreenToggleHandle(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    // 현재 화면 해상도와 일치하는 번호를 반환
    private int GetCurResolutionIndex()
    {
        for (int i = 0; i < _resolutionField.Length; ++i)
        {
            var resolution = _resolutions[i];
            if (Screen.width != resolution.x) continue;
            if (Screen.height != resolution.y) continue;
            return i;
        }
        return _resolutions.Length - 1; // 전체화면
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnEnable()
    {
        EscManager.Ins.Enter(this);
        // 초기 세팅
        _fullScreen.isOn = Screen.fullScreen;
        int curIndex = GetCurResolutionIndex();
        // 구독하기
        for (int i = 0; i < _resolutionField.Length; i++)
        {
            _resolutionField[i].SetIsOnWithoutNotify(i == curIndex); // Ui 초기 갱신
            int index = i; // 지역 변수에 복사
            _resolutionField[i].onValueChanged.AddListener(
                (isOn) =>
                {
                    ResolutionToggleHandle(index);
                }
            );
        }
        _fullScreen.onValueChanged.AddListener(FullScreenToggleHandle);
    }

    private void OnDisable()
    {
        // 구독 해제
        for (int i = 0; i < _resolutionField.Length; i++)
        {
            _resolutionField[i].onValueChanged.RemoveAllListeners();
        }
        _fullScreen.onValueChanged.RemoveAllListeners();
    }
    #endregion
}
