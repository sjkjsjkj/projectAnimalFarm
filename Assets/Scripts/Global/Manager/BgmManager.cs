/// <summary>
/// 씬이 전환될 때마다 재생하는 BGM을 변경합니다.
/// </summary>
public class BgmManager : Singleton<BgmManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }
        EventBus<OnSceneLoadEnd>.Subscribe(SceneChangeHandle);
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }

    private void SceneChangeHandle(OnSceneLoadEnd ctx)
    {
        ChangeBGM(ctx.nextScene);
    }

    private void ChangeBGM(EScene scene)
    {
        switch (scene)
        {
            case EScene.Title:
                USound.PlayBgm(Id.Bgm_Autumn_1);
                break;
            case EScene.Main:
                USound.PlayBgm(Id.Bgm_Spring_1);
                break;
            case EScene.Forest:
                USound.PlayBgm(Id.Bgm_Forest_1);
                break;
            case EScene.Cave:
                USound.PlayBgm(Id.Bgm_Cave_1);
                break;
            default:
                UDebug.Print($"등록되지 않은 씬이므로 기본 Bgm을 재생합니다.");
                USound.PlayBgm(Id.Bgm_Night_1);
                break;
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnDisable()
    {
        EventBus<OnSceneLoadEnd>.Unsubscribe(SceneChangeHandle);
    }
    #endregion
}
