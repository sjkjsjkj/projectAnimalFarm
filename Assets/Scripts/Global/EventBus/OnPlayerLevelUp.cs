/// <summary>
/// 플레이어의 기술 레벨이 증가했을 때
/// </summary>
public readonly struct OnPlayerLevelUp
{
    public readonly ELifeSkill skillType;
    public readonly int newLevel;

    public OnPlayerLevelUp(ELifeSkill skillType, int newLevel)
    {
        this.skillType = skillType;
        this.newLevel = newLevel;
    }

    public static void Publish(ELifeSkill skillType, int newLevel)
    {
        EventBus<OnPlayerLevelUp>.Publish(new(skillType, newLevel));
    }
}
