/// <summary>
/// 
/// </summary>
public readonly struct OnFarmStateChange
{
    public readonly EFarmlandState state;
    public readonly int pos;
    public readonly string seedId;
    public readonly int currentProgress;
    public OnFarmStateChange(EFarmlandState farmlansState, int pos , string seedId, int currentGrownProgress)
    {
        state = farmlansState;
        this.pos = pos;
        this.seedId = seedId;
        currentProgress = currentGrownProgress;
    }

    /// <param name="number">숫자</param>
    public static void Publish(EFarmlandState farmlansState, int pos, string seedId, int currentGrownProgress)
    {
        EventBus<OnFarmStateChange>.Publish(new OnFarmStateChange(farmlansState,pos,seedId, currentGrownProgress));
    }
}
