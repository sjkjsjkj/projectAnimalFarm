/// <summary>
/// 플레이어의 슬롯이 변경되었을 때
/// </summary>
public readonly struct OnPlayerSlotChanged
{
    public readonly int slotIndex;

    public OnPlayerSlotChanged(int itemId)
    {
        this.slotIndex = itemId;
    }

    /// <param name="slotIndex">ID</param>
    public static void Publish(int slotIndex)
    {
        EventBus<OnPlayerSlotChanged>.Publish(new(slotIndex));
    }
}
