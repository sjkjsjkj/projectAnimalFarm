/// <summary>
/// 동물이 배가고파지면 발생하는 이벤트
/// </summary>
public readonly struct OnHungry
{
    public static void Publish()
    {
        EventBus<OnHungry>.Publish(new OnHungry());
    }
}
