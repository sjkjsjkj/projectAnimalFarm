using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 해상도 변경 로직을 제공합니다.
/// </summary>
public class GameMenuScreen : BaseMono, IEscClosable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private GameMenu _gameMenu;
    [SerializeField] private Toggle[] _resolutionField = new Toggle[4];
    [SerializeField] private Toggle _fullScreen;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public bool CanCloseWithEsc => true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly Vector2Int[] _resolutions =
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),
    };
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public void CloseUi()
    {
        EscManager.Ins.Exit(this);

        if (_gameMenu == null)
        {
            UObject.SetActive(gameObject, false);
            return;
        }

        _gameMenu.OpenMenu();
    }

    public void CloseButton()
    {
        CloseUi();
        //EscManager.Ins.Exit(this);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void ResolutionToggleHandle(int index)
    {
        // 정의되지 않은 해상도
        if (index < 0 || index >= _resolutions.Length - 1) // 마지막 인덱스 포함
        {
            return;
        }
        // 
        int safe = Mathf.Clamp(index, 0, _resolutions.Length - 1);
        int x = _resolutions[safe].x;
        int y = _resolutions[safe].y;
        // bool이 아닌 Mode로 직접 설정해야 안정적
        FullScreenMode mode = _fullScreen.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(x, y, mode);
    }

    // 풀스크린 전환
    private void FullScreenToggleHandle(bool isFullScreen)
    {
        FullScreenMode mode = isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        // 전체화면으로 전환 → 현재 모니터 해상도 반영
        if (isFullScreen)
        {
            Resolution curResolution = Screen.currentResolution;
            Screen.SetResolution(curResolution.width, curResolution.height, mode);
        }
        // 창 모드로 전환
        else
        {
            int curIndex = GetCurResolutionIndex();
            // 사용자 정의 해상도
            if (curIndex < 0 || curIndex >= _resolutionField.Length - 1)
            {
                Screen.fullScreenMode = mode;
            }
            // 정의된 해상도
            else
            {
                Screen.SetResolution(_resolutions[curIndex].x, _resolutions[curIndex].y, mode);
            }
        }
    }

    // 현재 화면 해상도와 일치하는 번호를 반환
    private int GetCurResolutionIndex()
    {
        for (int i = 0; i < _resolutions.Length; ++i)
        {
            var resolution = _resolutions[i];
            if (Screen.width != resolution.x) continue;
            if (Screen.height != resolution.y) continue;
            return i;
        }
        return _resolutionField.Length - 1; // 사용자 정의
    }

    // 새로 갱신
    private void RefreshResolutionUi(OnResolutionChanged ctx)
    {
        int curIndex = GetCurResolutionIndex();
        for (int i = 0; i < _resolutionField.Length; i++)
        {
            _resolutionField[i].SetIsOnWithoutNotify(i == curIndex);
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        UDebug.IsNull(_gameMenu);
        UDebug.IsNull(_fullScreen);

        for (int i = 0; i < _resolutionField.Length; i++)
        {
            UDebug.IsNull(_resolutionField[i]);
        }
    }

    private void OnEnable()
    {
        // 해상도 변화 구독
        EventBus<OnResolutionChanged>.Subscribe(RefreshResolutionUi);
        // EscManager에 등록
        EscManager.Ins.Enter(this);
        // 초기 세팅
        bool isFullScreen = Screen.fullScreenMode != FullScreenMode.Windowed;
        _fullScreen.SetIsOnWithoutNotify(isFullScreen);
        int curIndex = GetCurResolutionIndex();
        // 구독하기
        for (int i = 0; i < _resolutionField.Length; i++)
        {
            _resolutionField[i].SetIsOnWithoutNotify(i == curIndex); // Ui 초기 갱신

            int index = i; // 지역 변수에 복사

            _resolutionField[i].onValueChanged.AddListener(
                (isOn) =>
                {
                    if (isOn == false) // 토글이 켜질 때만 반응
                    {
                        return;
                    }

                    ResolutionToggleHandle(index);
                }
            );
        }

        _fullScreen.onValueChanged.AddListener(FullScreenToggleHandle);
    }

    private void OnDisable()
    {
        EventBus<OnResolutionChanged>.Unsubscribe(RefreshResolutionUi);
        // 구독 해제
        for (int i = 0; i < _resolutionField.Length; i++)
        {
            if (_resolutionField[i] == null)
            {
                continue;
            }

            _resolutionField[i].onValueChanged.RemoveAllListeners();
        }

        if (_fullScreen != null)
        {
            _fullScreen.onValueChanged.RemoveAllListeners();
        }
    }
    #endregion
}
