using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class ShopInteractObject : BaseMono, IInteractable // 맵에 귀속될 NPC 이기 때문에 저장될 필요 없음
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("주제")]
    [SerializeField] private ShopSO _shopData;
    //[SerializeField] private UIShopPresenter _shopUI;

    [Header("상점 ID")]
    [SerializeField] private int _shopID;

    [Header("자동 닫기 설정")]
    [SerializeField] private float _autoCloseDistance = 3.0f;
    [SerializeField] private bool _printAutoCloseLog = false;
    #endregion

    #region 내부 변수
    private Shop _shop;
    private bool _isOpen = false;
    private NPCObject _npcObject;
    #endregion

    #region 내부메서드
    /// <summary>
    /// 현재 ShopManager와 ShopUI가 유효한지 검사합니다.
    /// </summary>
    /// <param name="shouldPrintLog">실패 시 로그 출력 여부</param>
    /// <returns>유효 여부</returns>
    private bool HasValidShopManager(bool shouldPrintLog)
    {
        if (ShopManager.Ins == null)
        {
            if (shouldPrintLog)
            {
                UDebug.Print("ShopInteractObject: ShopManager가 존재하지 않습니다.", LogType.Assert);
            }

            return false;
        }

        if (ShopManager.Ins.IsSettingFinish == false)
        {
            if (shouldPrintLog)
            {
                UDebug.Print("ShopInteractObject: ShopManager 초기화가 아직 완료되지 않았습니다.", LogType.Warning);
            }

            return false;
        }

        if (ShopManager.Ins.ShopUI == null)
        {
            if (shouldPrintLog)
            {
                UDebug.Print("ShopInteractObject: ShopUI가 아직 생성되지 않았습니다.", LogType.Warning);
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// 현재 상점 UI가 열려 있는지 반환합니다.
    /// </summary>
    /// <returns>열림 여부</returns>
    private bool IsShopUiOpen()
    {
        if (HasValidShopManager(false) == false)
        {
            return false;
        }

        return ShopManager.Ins.ShopUI.IsOpen;
    }

    /// <summary>
    /// 거리 자동 닫기 추적을 시작합니다.
    /// </summary>
    /// <param name="player">상호작용한 플레이어</param>
    private void StartAutoCloseTracking(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        if (UIAutoCloseManager.Ins == null)
        {
            UDebug.Print("ShopInteractObject: UIAutoCloseManager가 씬에 없습니다.", LogType.Warning);
            return;
        }

        if (HasValidShopManager(true) == false)
        {
            return;
        }

        ShopUI shopUi = ShopManager.Ins.ShopUI;

        UIAutoCloseManager.Ins.StartTracking(
            player.transform,
            transform,
            shopUi,
            shopUi.gameObject,
            _autoCloseDistance,
            true,
            _printAutoCloseLog,
            "Shop");
    }

    /// <summary>
    /// 거리 자동 닫기 추적을 중단합니다.
    /// </summary>
    private void StopAutoCloseTracking()
    {
        if (UIAutoCloseManager.Ins == null)
        {
            return;
        }

        UIAutoCloseManager.Ins.StopTracking();
    }
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
        if(player == null)
    {
            return;
        }

        //상점 UI 열면서 _shop 보냄.
        //상점 UI 에서는 _shop.SellItems 로 접근
        //throw new System.NotImplementedException();
        //UDebug.Print("상점 인터랙션");
        ShopManager.Ins.SetToggleShopUI(_shopID);
        _shop.ShowShopList();
        
        _npcObject.ShowDialog();

        if (IsShopUiOpen())
        {
            StartAutoCloseTracking(player);
            return;
        }

        StopAutoCloseTracking();
    }
    #endregion

    #region 메시지 함수
    protected override void Awake()
    {
        _shop = new Shop(_shopData.ShopItems);
        _shopID = ShopManager.Ins.RequestNewShop(_shopData);
        _npcObject = GetComponent<NPCObject>();
    }
    #endregion
}
