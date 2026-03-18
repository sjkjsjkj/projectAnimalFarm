/// <summary>
/// 플레이어의 스태미나가 변경되었을 때
/// </summary>
public readonly struct OnPlayerStaminaChanged
{
    public readonly float curStamina;
    public readonly float maxStamina;

    public OnPlayerStaminaChanged(float curStamina, float maxStamina)
    {
        this.curStamina = curStamina;
        this.maxStamina = maxStamina;
    }

    /// <param name="curStamina">현재 스태미나 수치</param>
    /// <param name="maxStamina">최대 스태미나 수치</param>
    public static void Publish(float curStamina, float maxStamina)
    {
        EventBus<OnPlayerStaminaChanged>.Publish(new(curStamina, maxStamina));
    }
}
