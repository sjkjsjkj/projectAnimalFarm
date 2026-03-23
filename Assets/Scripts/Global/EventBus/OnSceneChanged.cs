/// <summary>
/// 씬 변경이 시작되었을 때
/// </summary>
public readonly struct OnSceneChanged
{
    public readonly EScene scene;

    public OnSceneChanged(EScene scene)
    {
        this.scene = scene;
    }

    /// <param name="scene">씬 ID</param>
    public static void Publish(EScene scene)
    {
        EventBus<OnSceneChanged>.Publish(new OnSceneChanged(scene));
    }
}
