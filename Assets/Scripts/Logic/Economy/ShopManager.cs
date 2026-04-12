using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 싱글톤 클래스의 설계 의도입니다.
/// </summary>
public class ShopManager : GlobalSingleton<ShopManager>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private ShopUI _shopUIPrefab;

    private bool _isInitialized = false;
    private Transform _shopCanvasTr;
    private List<Shop> _shopList;
    private int _currentInteractShopId;
    private ShopUI _shopUI;
    private bool _isSettingFinish = false;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public ShopUI ShopUI => _shopUI;
    public bool IsSettingFinish => _isSettingFinish;


    public int RequestNewShop(ShopSO shopSO)
    {
        _shopList.Add(new Shop(shopSO.ShopItems));
        return _shopList.Count-1;
    }

    public void SetToggleShopUI(int id)
    {
        if (_shopCanvasTr == null)
        {
            _shopCanvasTr = UObject.Find(K.NAME_GLOBAL_CANVAS_ROOT).transform;
        }

        _currentInteractShopId = id;
        _shopUI.SetInfo(_shopList[id] );
        //_shopUI.RefreshInventoryUI(_inventoryList[id]);
        _shopUI.SetToggleUI();


    }


    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }

        _shopList = new List<Shop>();

        CollectPrefab();

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    private void CollectPrefab()
    {
        _shopUIPrefab = Resources.Load<ShopUI>("Prefab/UIShopView_");
        
        StartCoroutine(SceneLoadWaitCoroutine());
    }
    private IEnumerator SceneLoadWaitCoroutine()
    {
        while (true)
        {
            yield return null;
            GameObject tempCanvas = UObject.Find(K.NAME_GLOBAL_CANVAS_ROOT);
            if (tempCanvas != null)
            {
                _shopCanvasTr = tempCanvas.transform;
                break;
            }
        }
        MakeInventoryUIs();//인벤토리 UI들 생성 (인벤 / 창고 / 상점 각각 하나씩)

        _isSettingFinish = true;
    }

    //각 UI들을 생성하는 메서드.
    //이곳에서 생성된 UI로 각종 창고와 상점 / 플레이어의 인벤토리 UI를 보여 줌.
    private void MakeInventoryUIs()
    {
        if (_shopUIPrefab == null)
        {
            UDebug.Print("Global Prefab : PlayerInvenUI 찾을 수 없음. 확인", LogType.Assert);
            return;
        }

        //인벤토리 UI 활성화
        _shopUI = Instantiate(_shopUIPrefab);
        _shopUI.transform.SetParent(_shopCanvasTr);
        _shopUI .transform.localPosition = new Vector3(900, 0);
        _shopUI .gameObject.SetActive(false);

        
    }
    #endregion
}
