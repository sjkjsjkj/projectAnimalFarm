/// <summary>
/// 
/// </summary>
public readonly struct OnPlayerPlantingSeeds
{
    public readonly int number;

    public OnPlayerPlantingSeeds(int number)
    {
        this.number = number;
    }

    /// <param name="number">숫자</param>
    public static void Publish(int number)
    {
        EventBus<OnPlayerPlantingSeeds>.Publish(new OnPlayerPlantingSeeds(number));
    }
}
