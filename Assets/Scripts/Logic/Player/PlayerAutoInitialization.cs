/// <summary>
/// 플레이어의 글로벌 데이터 값이 초기화되지 않은 상태일 경우 자동으로 초기화합니다.
/// </summary>
public class PlayerAutoInitialization : BaseMono
{
    private void Start()
    {
        var provider = DataManager.Ins.Player;
        if (provider.IsEmpty())
        {
            var so = DatabaseManager.Ins.Player(Id.World_Player);
            provider.Initialize(so, K.PLAYER_SKILL_COUNT);
        }
    }
}
