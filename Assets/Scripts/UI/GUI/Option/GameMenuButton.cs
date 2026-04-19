using UnityEngine;

/// <summary>
/// 게임 메뉴 버튼 로직을 제공합니다.
/// </summary>
public class GameMenuButton : BaseMono, IEscClosable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private GameMenu _gameMenu;
    //[SerializeField] private GameMenuScreen _resolutionWindow;
    //[SerializeField] private GameMenuAudio _audioWindow;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public bool CanCloseWithEsc => true;
    #endregion


    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public void ResolutionButton()
    {
        UDebug.Print($"해상도 버튼 클릭");
        //UObject.SetActive(_resolutionWindow.gameObject, true);
        if (_gameMenu == null)
        {
            return;
        }

        _gameMenu.OpenResolutionWindow();
        USound.PlaySfx(Id.Sfx_Ui_Select_01);
    }
 
    public void AudioButton()
    {
        UDebug.Print($"오디오 버튼 클릭");
        //UObject.SetActive(_audioWindow.gameObject, true);
        if (_gameMenu == null)
        {
            return;
        }

        _gameMenu.OpenAudioWindow();
        USound.PlaySfx(Id.Sfx_Ui_Select_01);
    }

    public void TitleButton()
    {
        UDebug.Print($"타이틀 버튼 클릭");
        GameManager.Ins.LoadSceneAsyncWithFade((int)EScene.Title, 0f, 1.5f, 1f);
        USound.PlaySfx(Id.Sfx_Ui_Select_02);
    }

    public void ExitButton()
    {
        UDebug.Print($"게임 종료 버튼 클릭");
        USound.PlaySfx(Id.Sfx_Ui_Select_08);
        PersistenceManager.Ins.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 플레이 모드 종료
#else
        Application.Quit(); // 게임 종료
#endif
    }

    public void CloseUi()
    {
        if (_gameMenu != null)
        {
            UObject.SetActive(gameObject, false);
            //_gameMenu.CloseAllWindows();
            EscManager.Ins.Exit(this);
            return;
        }

        //UObject.SetActive(gameObject, false);
        //EscManager.Ins.Exit(this);
        _gameMenu.CloseAllWindows();
        USound.PlaySfx(Id.Sfx_Ui_Select_01);
    }

    public void CloseButton()
    {
        CloseUi();
        //EscManager.Ins.Exit(this);
    }
    #endregion

    private void OnEnable()
    {
        EscManager.Ins.Enter(this);
    }
}
