/// <summary>
/// UI가 상점 로직과 소통하기 위해 준수해야 할 규격
/// </summary>
public interface IShopUI
{
    /// <summary>
    /// 구매 버튼을 눌렀을 경우 이벤트 뿌리기
    /// </summary>
    event System.Action<string, int, int> OnBuyButtonPressed;

    /// <summary>
    /// 판매 버튼을 눌렀을 경우 이벤트 뿌리기
    /// </summary>
    event System.Action<string, int, int> OnSellButtonPressed;

    /// <summary>
    /// 구매가 성공했을 경우 호출당할 함수
    /// </summary>
    void BuySuccessHandle(string successMsg);

    /// <summary>
    /// 구매가 실패했을 경우 호출당할 함수
    /// </summary>
    void BuyFailureHandle(string failMsg);

    /// <summary>
    /// 판매가 성공했을 경우 호출당할 함수
    /// </summary>
    void SellSuccessHandle(string successMsg);

    /// <summary>
    /// 판매가 실패했을 경우 호출당할 함수
    /// </summary>
    void SellFailureHandle(string failMsg);
}
