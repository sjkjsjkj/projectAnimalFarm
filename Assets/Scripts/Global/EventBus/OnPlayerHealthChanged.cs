/// <summary>
/// 플레이어의 체력이 변경되었을 때
/// </summary>
public readonly struct OnPlayerHealthChanged
{
    public readonly float curHealth;
    public readonly float maxHealth;

    public OnPlayerHealthChanged(float curHealth, float maxHealth)
    {
        this.curHealth = curHealth;
        this.maxHealth = maxHealth;
    }

    public static void Publish(float curHealth, float maxHealth)
    {
        EventBus<OnPlayerStaminaChanged>.Publish(new(curHealth, maxHealth));
    }
}
