/// <summary>
/// 씬이 전환될 때마다 재생하는 BGM을 변경합니다.
/// </summary>
public class BgmManager : GlobalSingleton<BgmManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        EventBus<OnSceneLoadEnd>.Subscribe(SceneChangeHandle);
        EventBus<OnTimeChanged>.Subscribe(TimeChangeHandle);
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }

    private void SceneChangeHandle(OnSceneLoadEnd ctx)
    {
        ChangeBGM(ctx.nextScene);
    }

    private void TimeChangeHandle(OnTimeChanged ctx)
    {
        EScene curScene = GameManager.Ins.Scene;
        if (curScene == EScene.Main || curScene == EScene.Forest)
        {
            ChangeBGM(curScene);
        }
    }

    private void ChangeBGM(EScene scene)
    {
        switch (scene)
        {
            case EScene.Title:
                USound.PlayBgm(Id.Bgm_Autumn_1);
                UDebug.Print($"타이틀 BGM을 재생합니다.");
                break;
            case EScene.Main:
                // 낮
                if (TimeAndLight.IsDay)
                {
                    if (UMath.IsProbability(50f))
                    {
                        USound.PlayBgm(Id.Bgm_Spring_1);
                    }
                    else
                    {
                        USound.PlayBgm(Id.Bgm_Spring_2);
                    }
                }
                // 밤
                else
                {
                    if (UMath.IsProbability(50f))
                    {
                        USound.PlayBgm(Id.Bgm_Night_1);
                    }
                    else
                    {
                        USound.PlayBgm(Id.Bgm_Night_2);
                    }
                }
                UDebug.Print($"마을 BGM을 재생합니다.");
                break;
            case EScene.Forest:
                // 낮
                if (TimeAndLight.IsDay)
                {
                    if (UMath.IsProbability(50f))
                    {
                        USound.PlayBgm(Id.Bgm_Forest_3);
                    }
                    else
                    {
                        USound.PlayBgm(Id.Bgm_Forest_4);
                    }
                }
                // 밤
                else
                {
                    if (UMath.IsProbability(50f))
                    {
                        USound.PlayBgm(Id.Bgm_Winter_1);
                    }
                    else
                    {
                        USound.PlayBgm(Id.Bgm_Winter_2);
                    }
                }
                UDebug.Print($"숲 BGM을 재생합니다.");
                break;
            case EScene.Cave:
                if (UMath.IsProbability(50f))
                {
                    USound.PlayBgm(Id.Bgm_Cave_1);
                }
                else
                {
                    USound.PlayBgm(Id.Bgm_Cave_3);
                }
                UDebug.Print($"동굴 BGM을 재생합니다.");
                break;
            default:
                if (UMath.IsProbability(50f))
                {
                    USound.PlayBgm(Id.Bgm_Night_1);
                }
                else
                {
                    USound.PlayBgm(Id.Bgm_Night_2);
                }
                UDebug.Print($"등록되지 않은 씬이므로 기본 Bgm을 재생합니다.");
                break;
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnDisable()
    {
        EventBus<OnSceneLoadEnd>.Unsubscribe(SceneChangeHandle);
    }

    private void LateUpdate()
    {
        if (USound.IsPlayingBgm())
        {
            return;
        }
        ChangeBGM(GameManager.Ins.Scene);
    }
    #endregion
}
