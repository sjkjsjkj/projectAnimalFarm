/// <summary>
/// 자동으로 다음 씬으로 이동합니다. (부트 씬 용도)
/// </summary>
public class AutoNextScene : BaseMono
{
    private void OnProgress(float progress)
    {
        UDebug.Print($"다음 씬으로 로딩 중입니다. {progress * 100}%");
    }

    private void OnLoadComplete()
    {
        UDebug.Print($"씬 로드가 완료되었습니다.");
    }

    private void Start()
    {
        var gm = GameManager.Ins;
        int nextSceneIndex = (int)gm.Scene + 1;
        gm.LoadSceneAsync(nextSceneIndex, OnLoadComplete, OnProgress);
    }
}
