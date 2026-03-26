/// <summary>
/// 플레이어가 손에 든 아이템을 변경했을 때
/// </summary>
public readonly struct OnPlayerHeldItemChanged
{
    public readonly string itemId;

    public OnPlayerHeldItemChanged(string itemId)
    {
        this.itemId = itemId;
    }

    /// <param name="itemId">ID</param>
    public static void Publish(string itemId)
    {
        EventBus<OnPlayerHeldItemChanged>.Publish(new(itemId));
    }
}
