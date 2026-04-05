/// <summary>
/// UI가 제작 로직과 소통하기 위해 준수해야 할 규격
/// </summary>
public interface ICraftUI
{
    /// <summary>
    /// 제작할 아이템을 클릭했을 경우 발행할 이벤트
    /// </summary>
    event System.Action<string> OnItemClicked;

    /// <summary>
    /// 제작 버튼을 눌렀을 경우 발행할 이벤트
    /// </summary>
    event System.Action<string> OnCraftButtonPressed;

    /// <summary>
    /// 제작이 성공했을 경우 호출할 함수
    /// </summary>
    void CraftSuccessHandle(string successMsg);

    /// <summary>
    /// 제작이 실패했을 경우 호출할 함수
    /// </summary>
    void CraftFailureHandle(string failMsg);

    /// <summary>
    /// 제작 아이템을 클릭했을 경우 호출당할 함수
    /// 현재 선택한 아이템이 제작 가능한지 호출당할 함수
    /// </summary>
    void ReceiveMaterialsHandle(out WorkbenchReturnStruct[] works);
}
