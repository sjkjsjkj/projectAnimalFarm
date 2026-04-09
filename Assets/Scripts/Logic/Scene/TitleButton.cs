/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TitleButton : BaseMono
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void GameStart()
    {
        GameManager.Ins.LoadSceneAsyncWithFade((int)EScene.Main);
    }
    #endregion
}
