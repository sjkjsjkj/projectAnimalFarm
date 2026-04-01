/// <summary>
/// 플레이어가 아이템(도구) 사용 키를 눌렀을 때
/// 키를 눌렀을 때 1회 발행
/// </summary>
public readonly struct OnPlayerUse
{
    public static void Publish()
    {
        EventBus<OnPlayerUse>.Publish(new OnPlayerUse());
    }
}
