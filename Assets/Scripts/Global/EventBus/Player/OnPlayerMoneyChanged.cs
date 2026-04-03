/// <summary>
/// 플레이어의 돈이 변경되었을 때
/// </summary>
public readonly struct OnPlayerMoneyChanged
{
    public readonly int curMoney;

    public OnPlayerMoneyChanged(int curMoney)
    {
        this.curMoney = curMoney;
    }

    /// <param name="curMoney">현재 보유한 돈</param>
    public static void Publish(int curMoney)
    {
        EventBus<OnPlayerMoneyChanged>.Publish(new(curMoney));
    }
}
