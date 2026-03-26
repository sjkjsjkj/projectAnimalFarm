/// <summary>
/// 플레이어의 생활 기술 레벨이 증가했을 때
/// </summary>
public readonly struct OnPlayerSkillExpUp
{
    public readonly ELifeSkill skillType;
    public readonly int curExp;
    public readonly int addExp;

    public OnPlayerSkillExpUp(ELifeSkill skillType, int curExp, int addExp)
    {
        this.skillType = skillType;
        this.curExp = curExp;
        this.addExp = addExp;
    }

    /// <param name="skillType">생활 기술</param>
    /// <param name="curExp">현재 경험치</param>
    /// <param name="addExp">획득한 경험치</param>
    public static void Publish(ELifeSkill skillType, int curExp, int addExp)
    {
        EventBus<OnPlayerSkillExpUp>.Publish(new(skillType, curExp, addExp));
    }
}
