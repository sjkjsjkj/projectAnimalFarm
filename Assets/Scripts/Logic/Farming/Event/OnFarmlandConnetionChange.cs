/// <summary>
/// 경작지가 주변의 경작지 중, 상태가 같은 경작지와 연결되었을 때 발생하는 이벤트
/// </summary>
public readonly struct OnFarmlandConnetionChange
{
    public readonly uint connectionDir;
    public readonly EFarmlandState state;
    public readonly int pos;

    public OnFarmlandConnetionChange(uint connectionDir, EFarmlandState state, int pos)
    {
        this.connectionDir = connectionDir;
        this.state = state;
        this.pos = pos;
    }

    /// <param name="connectionDir">변경된 연결 방향</param>
    public static void Publish(uint connectionDir, EFarmlandState state, int pos)                   
    {
        EventBus<OnFarmlandConnetionChange>.Publish(new OnFarmlandConnetionChange(connectionDir, state, pos));
    }
}
