/// <summary>
/// 플레이어 상태 로직이 UI와 소통하기 위해 준수해야 할 규격
/// </summary>
public interface IStatusLogical
{
    /// <summary>
    /// 플레이어가 보유한 도구 중 해당 타입과 일치하는 도구 ID를 배열로 반환하는 함수
    /// </summary>
    string[] GetTools(EType toolType);

    /// <summary>
    /// 아이템 판매 요청이 들어왔을 경우 처리하여 판매 성공 여부를 반환하는 함수
    /// </summary>
    bool TrySwapTool(string toolId, out string message);
}
