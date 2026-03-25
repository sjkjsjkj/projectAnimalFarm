/// <summary>
/// 플레이어의 목마름 수치가 변경되었을 때
/// </summary>
public readonly struct OnPlayerThirstChanged
{
    public readonly float curThirst;
    public readonly float maxThirst;

    public OnPlayerThirstChanged(float curStamina, float maxStamina)
    {
        this.curThirst = curStamina;
        this.maxThirst = maxStamina;
    }

    /// <param name="curThirst">현재 목마름 수치</param>
    /// <param name="maxThirst">최대 목마름 수치</param>
    public static void Publish(float curThirst, float maxThirst)
    {
        EventBus<OnPlayerThirstChanged>.Publish(new(curThirst, maxThirst));
    }
}
