/// <summary>
/// 상점 로직이 UI와 소통하기 위해 준수해야 할 규격
/// </summary>
public interface IShopLogical
{
    // 아이템 구매 요청이 들어왔을 경우 처리하여 구매 성공 여부를 반환하는 함수
    bool TryBuyItem(string itemId, int buyPrice, int amount, out string message);

    // 아이템 판매 요청이 들어왔을 경우 처리하여 판매 성공 여부를 반환하는 함수
    bool TrySellItem(string itemId, int sellPrice, int amount, out string failMessage);
}
