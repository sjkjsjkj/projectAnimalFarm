/// <summary>
/// 플레이어의 생활 기술 레벨이 증가했을 때
/// </summary>
public readonly struct OnPlayerSkillLevelUp
{
    public readonly ELifeSkill skillType;
    public readonly int newLevel;

    public OnPlayerSkillLevelUp(ELifeSkill skillType, int newLevel)
    {
        this.skillType = skillType;
        this.newLevel = newLevel;
    }

    /// <param name="skillType">생활 기술</param>
    /// <param name="newLevel">현재 레벨</param>
    public static void Publish(ELifeSkill skillType, int newLevel)
    {
        EventBus<OnPlayerSkillLevelUp>.Publish(new(skillType, newLevel));
    }
}
