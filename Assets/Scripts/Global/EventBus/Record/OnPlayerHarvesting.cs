/// <summary>
/// 
/// </summary>
public readonly struct OnPlayerHarvesting
{
    public readonly int number;

    public OnPlayerHarvesting(int number)
    {
        this.number = number;
    }

    /// <param name="number">숫자</param>
    public static void Publish(int number)
    {
        EventBus<OnPlayerHarvesting>.Publish(new OnPlayerHarvesting(number));
    }
}
