
using UnityEngine;

/// <summary>
/// 게임 메뉴 버튼 로직을 제공합니다.
/// </summary>
public class GameMenuButton : BaseMono, IEscClosable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private GameMenuScreen _resolutionWindow;
    [SerializeField] private GameMenuAudio _audioWindow;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void ResolutionButton()
    {
        UDebug.Print($"해상도 버튼 클릭");
        UObject.SetActive(_resolutionWindow.gameObject, true);
    }
 
    public void AudioButton()
    {
        UDebug.Print($"오디오 버튼 클릭");
        UObject.SetActive(_audioWindow.gameObject, true);
    }

    public void TitleButton()
    {
        UDebug.Print($"타이틀 버튼 클릭");
        GameManager.Ins.LoadSceneAsync((int)EScene.Title, CallbackHandle, ProgressHandle);
    }

    public void ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 플레이 모드 종료
#else
        Application.Quit(); // 게임 종료
#endif
        UDebug.Print($"해상도 버튼 클릭");
    }

    public void CloseUi()
    {
        UObject.SetActive(gameObject, false);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void CallbackHandle()
    {

    }

    private void ProgressHandle(float onProgress)
    {

    }
    #endregion

    private void OnEnable()
    {
        EscManager.Ins.Enter(this);
    }
}
