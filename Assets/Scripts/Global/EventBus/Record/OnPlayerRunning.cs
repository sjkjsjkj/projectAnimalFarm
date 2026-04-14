/// <summary>
/// 
/// </summary>
public readonly struct OnPlayerRunning
{
    public readonly int number;

    public OnPlayerRunning(int number)
    {
        this.number = number;
    }

    /// <param name="number">숫자</param>
    public static void Publish(int number)
    {
        EventBus<OnPlayerRunning>.Publish(new OnPlayerRunning(number));
    }
}
