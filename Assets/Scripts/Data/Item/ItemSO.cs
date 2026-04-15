using UnityEngine;

/// <summary>
/// 인벤토리에 존재할 수 있는 아이템이 가지는 정적 데이터입니다.
/// </summary>
public abstract class ItemSO : UnitSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("아이템 기본 정보")]
    [SerializeField] protected int _sellPrice = 100; // 판매 가격
    [SerializeField] protected int _buyPrice = 200; // 구매 가격
    [SerializeField] protected int _maxStack = 64; // 최대 중첩 수

    // 인터페이스가 아닌 SO인 이유는 SO는 인스펙터창에서 넣고 빼는게 가능하기 때문에 개발 기간을 줄이기 위함.
    [SerializeField] protected ItemUseContextSO[] _itemUseContexts; 
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public ItemUseContextSO[] ItemUseContext => _itemUseContexts;
    public int SellPrice => _sellPrice;
    public int BuyPrice => _buyPrice;
    public int MaxStack => _maxStack;

    public void Use()
    {
        for (int i = 0; i < _itemUseContexts.Length; i++)
        {
            _itemUseContexts[i].TryUse();
        }
    }
    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_sellPrice < 0) return false;
        if (_buyPrice < 0) return false;
        if (_buyPrice > 0 && _sellPrice > _buyPrice) return false;
        if (_maxStack <= 0) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    // 인스펙터 변수 유효성 검사
    protected override void OnValidate()
    {
        base.OnValidate();
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {_id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
