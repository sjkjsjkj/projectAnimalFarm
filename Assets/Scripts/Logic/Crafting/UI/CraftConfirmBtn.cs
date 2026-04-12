using System;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class CraftConfirmBtn : BaseMono
{
    public event Action OnCraftConfirmBtnClick;

    public void CraftConfirmBtnClick()
    {
        OnCraftConfirmBtnClick?.Invoke();
    }
}
