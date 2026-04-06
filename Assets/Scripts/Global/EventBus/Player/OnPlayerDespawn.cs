/// <summary>
/// 플레이어가 삭제되었을 때 (사망, 씬 이동)
/// </summary>
public readonly struct OnPlayerDespawn
{
    public static void Publish()
    {
        EventBus<OnPlayerDespawn>.Publish(new());
    }
}
