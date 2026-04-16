using UnityEngine;

/// <summary>
/// 상점의 UI 입니다.
/// </summary>
public class ShopUI : BaseMono, IEscClosable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private UIShopSlotItem _sellItemSlotPrefab;
    [SerializeField] private Transform _sellItemSlotTransform;
    #endregion.

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private UIShopSlotItem[] _uiShopSlotItems;
    private ItemSO[] _sellItems;
    private Shop _shop;
    private bool _isOpen=false;

    public bool IsOpen => _isOpen;

    public bool CanCloseWithEsc => true;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void SetInfo(Shop shop)
    {
        _shop = shop;
        _sellItems = _shop.SellItems;
        _uiShopSlotItems = new UIShopSlotItem[_sellItems.Length];

        SettingSlot();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void SettingSlot()
    {
        for (int i = _sellItemSlotTransform.childCount - 1; i >=0; i--)
        {
            Destroy(_sellItemSlotTransform.GetChild(i).gameObject);
        }

        for (int i = 0; i < _sellItems.Length; i++)
        {
            _uiShopSlotItems[i] = Instantiate(_sellItemSlotPrefab, _sellItemSlotTransform);
            _uiShopSlotItems[i].Setup(i, _sellItems[i].Image, HandleClickBuy, HandleClickSell, _sellItems[i].SellPrice, _sellItems[i].BuyPrice);
        }
    }
    private void HandleClickBuy(int slotIndex)
    {
        if (_shop.TryBuyItem(_sellItems[slotIndex].Id, _sellItems[slotIndex].SellPrice, 1, out string message))
        {
            USound.PlaySfx(Id.Sfx_Other_Success_2);
            //ShowFeedback(message, EFeedbackMessageType.Success);
            return;
        }
        USound.PlaySfx(Id.Sfx_Other_Alert_2);
        //ShowFeedback(message, EFeedbackMessageType.Failure);
    }

    private void HandleClickSell(int slotIndex)
    {
        
        if (_shop.TrySellItem(_sellItems[slotIndex].Id, _sellItems[slotIndex].SellPrice, 1, out string message))
        {
            USound.PlaySfx(Id.Sfx_Other_Success_2);
            //ShowFeedback(message, EFeedbackMessageType.Success);
            return;
        }
        USound.PlaySfx(Id.Sfx_Other_Alert_2);
    }

    public virtual void SetToggleUI()
    {
        _isOpen = !_isOpen;
        if (_isOpen)
        {
            USound.PlaySfx(Id.Sfx_Ui_ChestOpen_2);
            ShowUI();
        }
        else
        {
            USound.PlaySfx(Id.Sfx_Ui_ChestClosed_2);
            CloseUI();
        }
        //UDebug.Print($"current Stats : {_isOpen}");
    }
    public void ShowUI()
    {
        gameObject.SetActive(true);
        EscManager.Ins.Enter(this); // 추가
    }
    public virtual void CloseUI()
    {
        EscManager.Ins.Exit(this); // 추가
        gameObject.SetActive(false);
    }

    public void CloseUi()
    {
        // ESC가 눌렸을 때 EscManager가 호출하는 함수 추가 
        _isOpen = false;
        USound.PlaySfx(Id.Sfx_Ui_ChestClosed_2);
        CloseUI();
    }

    /// <summary>
    /// 설정 창을 엽니다.
    /// 이미 열려 있는 경우 스택 재등록만 수행합니다.
    /// </summary>
    //public void Open()
    //{
    //    if (_windowRoot == null)
    //    {
    //        return;
    //    }

    //    if (_windowRoot.activeSelf == false)
    //    {
    //        _windowRoot.SetActive(true);
    //    }

    //    UIWindowStackManager manager = GetStackManager();
    //    if (manager != null)
    //    {
    //        manager.RegisterWindow(this);
    //    }
    //}

    /// <summary>
    /// 설정 창을 닫습니다.
    /// </summary>
    //public void Close()
    //{
    //    UIWindowStackManager manager = GetStackManager();
    //    if (manager != null)
    //    {
    //        manager.UnregisterWindow(this);
    //    }

    //    if (_windowRoot == null)
    //    {
    //        return;
    //    }

    //    if (_windowRoot.activeSelf == true)
    //    {
    //        _windowRoot.SetActive(false);
    //    }
    //}
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
