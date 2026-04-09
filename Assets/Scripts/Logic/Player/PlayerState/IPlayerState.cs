/// <summary>
/// 모든 플레이어 상태가 구현해야 하는 인터페이스.
/// </summary>
public interface IPlayerState
{
    bool Enter(in PlayerContext context);
    bool Frame(in PlayerContext context);
    void Exit(in PlayerContext context);
}
