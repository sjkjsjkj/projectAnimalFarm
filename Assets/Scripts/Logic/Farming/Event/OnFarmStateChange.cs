/// <summary>
/// 
/// </summary>
public readonly struct OnFarmStateChange
{
    public readonly EFarmlandState state;
    public readonly int pos;
    public readonly string seedId;
    public OnFarmStateChange(EFarmlandState farmlansState, int pos , string seedId)
    {
        state = farmlansState;
        this.pos = pos;
        this.seedId = seedId;
    }

    /// <param name="number">숫자</param>
    public static void Publish(EFarmlandState farmlansState, int pos, string seedId)
    {
        EventBus<OnFarmStateChange>.Publish(new OnFarmStateChange(farmlansState,pos,seedId));
    }
}
