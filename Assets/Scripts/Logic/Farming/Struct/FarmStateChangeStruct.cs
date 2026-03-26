/// <summary>
/// 경작지의 상태가 변경 될 때, 이벤트로 넘겨줄 구조체 입니다.
/// </summary>
public struct FarmStateChangeStruct
{
    public readonly EFarmlandState state;
    public readonly int pos;
    public readonly string seedId;
    public readonly int currentProgress;
    public FarmStateChangeStruct(EFarmlandState farmlansState, int pos, string seedId, int currentGrownProgress)
    {
        state = farmlansState;
        this.pos = pos;
        this.seedId = seedId;
        currentProgress = currentGrownProgress;
    }
    public void ShowStruct()
    {
        UDebug.Print($"State : {state} \nPos : {pos} \nseedId : {seedId} \nCurrentProgress : {currentProgress}");
    }
}
