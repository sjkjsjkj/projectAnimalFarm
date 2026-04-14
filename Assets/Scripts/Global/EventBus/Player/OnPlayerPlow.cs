/// <summary>
/// 플레이어가 경작했을 때
/// </summary>
public readonly struct OnPlayerPlow
{
    public static void Publish()
    {
        EventBus<OnPlayerPlow>.Publish(new OnPlayerPlow());
    }
}
