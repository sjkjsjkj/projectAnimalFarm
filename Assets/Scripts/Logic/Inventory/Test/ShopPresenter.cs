using UnityEngine;

/// <summary>
/// 상점 UI와 로직을 스스로 수집하여 중간 다리를 연결해주는 컴포넌트
/// </summary>
public class ShopPresenter : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("인터페이스를 준수하는 각 컴포넌트 참조 연결")]
    [SerializeField] private BaseMono _shopUiMono;
    [SerializeField] private BaseMono _shopLogicalMono;
    #endregion

    private IShopUI _shopUi;
    private IShopLogical _shopLogical;

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 구매 버튼을 눌렀을 때
    private void BuyButtonPressedHandle(string itemId, int price, int amount)
    {
        if(_shopUi == null || _shopLogical == null) return;
        // 판매 성공
        if (_shopLogical.TryBuyItem(itemId, price, amount, out string message))
        {
            _shopUi.BuySuccessHandle(message);
        }
        else // 판매 실패
        {
            _shopUi.BuyFailureHandle(message);
        }
    }

    // 판매 버튼을 눌렀을 때
    private void SellButtonPressedHandle(string itemId, int price, int amount)
    {
        if (_shopUi == null || _shopLogical == null) return;
        // 판매 성공
        if (_shopLogical.TrySellItem(itemId, price, amount, out string message))
        {
            _shopUi.SellSuccessHandle(message);
        }
        else // 판매 실패
        {
            _shopUi.SellFailureHandle(message);
        }
    }

    private bool TryGetShopUi()
    {
        if (_shopUiMono == null)
        {
            UDebug.Print($"인스펙터에 상점 UI가 등록되지 않았습니다.", LogType.Assert);
            return false;
        }
        if (!_shopUiMono.TryGetComponent(out _shopUi))
        {
            UDebug.Print($"인스펙터에 등록된 오브젝트에 IShopUI 인터페이스가 없습니다.", LogType.Assert);
            return false;
        }
        return true;
    }

    private bool TryGetShopLogic()
    {
        if (_shopLogicalMono == null)
        {
            UDebug.Print($"인스펙터에 상점 로직이 등록되지 않았습니다.", LogType.Assert);
            return false;
        }
        if (!_shopLogicalMono.TryGetComponent(out _shopLogical))
        {
            UDebug.Print($"인스펙터에 등록된 오브젝트에 IShopLogical 인터페이스가 없습니다.", LogType.Assert);
            return false;
        }
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        if (TryGetShopUi())
        {
            _shopUi.OnBuyButtonPressed += BuyButtonPressedHandle;
            _shopUi.OnSellButtonPressed += SellButtonPressedHandle;
        }
        TryGetShopLogic();
    }
    #endregion
}
