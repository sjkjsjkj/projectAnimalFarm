/// <summary>
/// 
/// </summary>
public readonly struct OnPlayerWalking
{
    public readonly int number;

    public OnPlayerWalking(int number)
    {
        this.number = number;
    }

    /// <param name="number">숫자</param>
    public static void Publish(int number)
    {
        EventBus<OnPlayerWalking>.Publish(new OnPlayerWalking(number));
    }
}
