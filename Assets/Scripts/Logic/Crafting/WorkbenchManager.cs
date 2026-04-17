using System.Collections;
using UnityEngine;

/// <summary>
/// 싱글톤 클래스의 설계 의도입니다.
/// </summary>
public class WorkbenchManager : GlobalSingleton<WorkbenchManager>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private WorkbenchUI _workbenchUIPrefab;

    private bool _isInitialized = false;
    private Transform _workbenchCanvasTr;
    private WorkbenchUI _workbenchUI;
    private bool _isSettingFinish = false;
    private Workbench _workbench;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public WorkbenchUI WorkbenchUI => _workbenchUI;
    public Workbench Workbench => _workbench;
    public bool IsSettingFinish => _isSettingFinish;

    public void SetToggleShopUI(int id)
    {
        if (_workbenchCanvasTr == null)
        {
            _workbenchCanvasTr = UObject.Find(K.NAME_GLOBAL_CANVAS_ROOT).transform;
        }

        _workbenchUI.SetToggleUI();

    }


    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        CollectPrefab();

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    private void CollectPrefab()
    {
        StartCoroutine(SceneLoadWaitCoroutine());
    }
    private IEnumerator SceneLoadWaitCoroutine()
    {
        bool isReady = false;
        while (true)
        {
            yield return null;
            GameObject tempCanvas = UObject.Find(K.NAME_GLOBAL_CANVAS_ROOT);

            if(tempCanvas!=null && DatabaseManager.Ins.IsInit)
            {
                _workbenchCanvasTr = tempCanvas.transform;
                isReady = true;
            }
            if(isReady)
            {
                break;
            }
        }

        _workbench = new Workbench();

        _workbenchUIPrefab = Resources.Load<WorkbenchUI>("Prefab/CraftPanelUI");

        MakeInventoryUIs();//인벤토리 UI들 생성 (인벤 / 창고 / 상점 각각 하나씩)

        _isSettingFinish = true;
    }

    //각 UI들을 생성하는 메서드.
    //이곳에서 생성된 UI로 각종 창고와 상점 / 플레이어의 인벤토리 UI를 보여 줌.
    private void MakeInventoryUIs()
    {
        if (_workbenchUIPrefab == null)
        {
            UDebug.Print("Global Prefab : CraftPanelUI 찾을 수 없음. 확인", LogType.Assert);
            return;
        }

        //인벤토리 UI 활성화
        _workbenchUI = Instantiate(_workbenchUIPrefab);

        if(_workbench == null)
        {
            UDebug.Print("워크벤치가 없네유");
        }

        _workbenchUI.SetInfo(_workbench);
        _workbenchUI.transform.SetParent(_workbenchCanvasTr);
        RectTransform rectTr = _workbenchUI.GetComponent<RectTransform>();
        UObject.ResetRect(rectTr);
        _workbenchUI.gameObject.SetActive(false);


    }
    #endregion
}
