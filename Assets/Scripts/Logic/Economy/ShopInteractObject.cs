using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class ShopInteractObject : BaseMono, IInteractable // 맵에 귀속될 NPC 이기 때문에 저장될 필요 없음
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("주제")]
    [SerializeField] private ShopSO _shopData;
    #endregion

    #region 내부 변수
    private Shop _shop;
    private bool _isOpen = false;
    #endregion

    #region 내부메서드
    
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool CanInteract(GameObject player)
    {
        //ToDo 상점 UI를 관리하는 매니저가 생기면 거기서 UI가 켜졌는지 안켜졌는지를 받아올 계획.
        return !_isOpen;
    }

    public string GetMessage()
    {
        UDebug.Print($"상점 열기");
        return "상점 열기";
    }

    public void Interact(GameObject player)
    {
        //상점 UI 열면서 _shop 보냄.
        //상점 UI 에서는 _shop.SellItems 로 접근
        //throw new System.NotImplementedException();
        //UDebug.Print("상점 인터랙션");
        _shop.ShowShopList();
    }
    #endregion

    #region 메시지 함수
    protected override void Awake()
    {
        _shop = new Shop(_shopData.ShopItems);
    }
    #endregion
}
