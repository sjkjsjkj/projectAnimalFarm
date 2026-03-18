/// <summary>
/// 계절이 바뀌었을 때
/// </summary>
public readonly struct OnSeasonChanged
{
    public readonly ESeason newSeason;

    public OnSeasonChanged(ESeason newSeason)
    {
        this.newSeason = newSeason;
    }

    /// <param name="newSeason">새로운 계절 ID</param>
    public static void Publish(ESeason newSeason)
    {
        EventBus<OnSeasonChanged>.Publish(new(newSeason));
    }
}
