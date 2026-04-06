using UnityEngine;

/// <summary>
/// 상점 UI와 로직을 스스로 수집하여 중간 다리를 연결해주는 컴포넌트
/// </summary>
public class ShopPresenter : BaseMono
{
    [Header("인터페이스를 준수하는 각 컴포넌트 참조 연결")]
    [SerializeField] private BaseMono _shopUiMono;
    [SerializeField] private BaseMono _shopLogicalMono;

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

    // 인스펙터 유효성 검사 및 내부 변수 작성하기
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

    // 인스펙터 유효성 검사 및 내부 변수 작성하기
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
    private void OnEnable()
    {
        if (TryGetShopUi())
        {
            _shopUi.OnBuyButtonPressed += BuyButtonPressedHandle;
            _shopUi.OnSellButtonPressed += SellButtonPressedHandle;
        }
        TryGetShopLogic();
    }

    private void OnDisable()
    {
        if (_shopUi != null)
        {
            _shopUi.OnBuyButtonPressed -= BuyButtonPressedHandle;
            _shopUi.OnSellButtonPressed -= SellButtonPressedHandle;
        }
    }

    // 인스펙터 편의성
    protected void Reset()
    {
        // 비활성화된 객체를 포함하여 탐색
        BaseMono[] monos = FindObjectsOfType<BaseMono>(true);
        // 인터페이스를 준수하는 컴포넌트 탐색
        int length = monos.Length;
        for (int i = 0; i < length; ++i)
        {
            BaseMono mono = monos[i];
            if(_shopUiMono == null && mono is ICraftUI)
            {
                UDebug.Print($"상점 UI 컴포넌트를 자동 탐색했습니다.");
                _shopUiMono = mono;
            }
            if(_shopLogicalMono == null && mono is IShopLogical)
            {
                UDebug.Print($"상점 로직 컴포넌트를 자동 탐색했습니다.");
                _shopLogicalMono = mono;
            }
            if(_shopUiMono != null && _shopLogicalMono != null)
            {
                UDebug.Print($"필요한 상점 컴포넌트 탐색을 완료했습니다.");
                return;
            }
        }
    }
    #endregion
}
