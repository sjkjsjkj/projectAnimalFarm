/// <summary>
/// 플레이어의 스태미나가 변경되었을 때
/// </summary>
public readonly struct OnPlayerHungerChanged
{
    public readonly float curHunger;
    public readonly float maxHunger;

    public OnPlayerHungerChanged(float curHunger, float maxHunger)
    {
        this.curHunger = curHunger;
        this.maxHunger = maxHunger;
    }

    /// <param name="curHunger">현재 배고픔 수치</param>
    /// <param name="maxHunger">최대 배고픔 수치</param>
    public static void Publish(float curHunger, float maxHunger)
    {
        EventBus<OnPlayerHungerChanged>.Publish(new(curHunger, maxHunger));
    }
}
