/// <summary>
/// UI가 상점 로직과 소통하기 위해 준수해야 할 규격
/// </summary>
public interface IShopUI
{
    event System.Action<string, int, int> OnBuyButtonPressed;
    event System.Action<string, int, int> OnSellButtonPressed;

    // 성공 및 실패 효과 작성에 매개변수가 더 필요하다면 추가를 고려 가능
    void BuySuccessHandle(string successMsg);
    void BuyFailureHandle(string failMsg);
    void SellSuccessHandle(string successMsg);
    void SellFailureHandle(string failMsg);
}
