/// <summary>
/// 자동으로 다음 씬으로 이동합니다. (부트 씬 용도)
/// </summary>
public class AutoNextScene : BaseMono
{
    private void Start()
    {
        var gm = GameManager.Ins;
        int nextSceneIndex = (int)gm.Scene + 1;
        gm.LoadSceneAsyncWithFade((int)nextSceneIndex);
    }
}
