/// <summary>
/// UI가 플레이어 상태 로직과 소통하기 위해 준수해야 할 규격
/// </summary>
public interface IStatusUI
{
    /// <summary>
    /// 도구 슬롯 클릭했을 경우 이벤트 발행
    /// </summary>
    event System.Action<EType> OnToolSlotClicked;

    /// <summary>
    /// 도구 슬롯의 리스트에서 보유한 도구를 클릭했을 경우 이벤트 발행
    /// </summary>
    event System.Action<string> OnToolItemClicked;

    /// <summary>
    /// 도구 슬롯을 클릭했을 경우 호출당할 함수
    /// </summary>
    void ToolSlotClickedHandle(out string[] toolIds);

    /// <summary>
    /// 도구 아이템을 클릭하여 장비한 도구가 교체되었을 경우 호출당할 함수
    /// </summary>
    void SwapToolSuccessHandle(string successMsg);

    /// <summary>
    /// 도구 아이템을 클릭했지만 도구를 교체하지 못했을 경우 호출당할 함수
    /// </summary>
    void SwapToolFailureHandle(string failMsg);
}
