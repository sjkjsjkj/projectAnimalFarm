using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 제작대 UI
/// </summary>
public class WorkbenchUI : BaseMono, ICraftUI, IEscClosable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────\
    [Header("제작대 UI")]

    [Header("레시피페이지UI")]
    [SerializeField] private CraftRecipeBtn _craftRecipeBtnPrefab;
    [SerializeField] private Transform _recipeBtnPage;

    [Header("상세페이지 UI")]
    [SerializeField] private CraftRequireItemList _craftRequireItemListPrefab;
    [SerializeField] private Transform _requireItemPage;

    [Header("버튼 영역 UI")]
    [SerializeField] private Image _cantBtn;
    [SerializeField] private CraftConfirmBtn _confirmBtn;
    [SerializeField] private Transform _btnPage;

    [Header("테스트")]
    [SerializeField] private Workbench _workbench;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    //private Workbench _workbench;
    private ECraftableItemType _currentChoiceCategory = ECraftableItemType.Axe;
    private bool _isOpen;
    private int _lastestChoicedBtnIdx=-1;
    //private List<RecipeSO> _currentSelectCategoryRecipies;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public event Action<string> OnItemClicked;
    public event Action<string> OnCraftButtonPressed;

    public void SetInfo(Workbench workbench)
    {
        UDebug.Print("셋인포");
        _workbench = workbench;

        _workbench.OnChenageRecipe -= SetInfoPage;
        _workbench.OnChenageRecipe += SetInfoPage;

        _confirmBtn.OnCraftConfirmBtnClick -= ConfirmButtonHandler;
        _confirmBtn.OnCraftConfirmBtnClick += ConfirmButtonHandler;

        CategoryBtnClick(_currentChoiceCategory);

    }
    #region 인터페이스
    public void CraftFailureHandle(string failMsg)
    {
        throw new NotImplementedException();
    }

    public void CraftSuccessHandle(string successMsg)
    {
        throw new NotImplementedException();
    }

    public void ReceiveMaterialsHandle(out WorkbenchReturnStruct[] works)
    {
        throw new NotImplementedException();
    }

    #endregion
    public void RecipeBtnClick(ECraftableItemType curChoiceCategory, int idx)
    {
        _workbench.SetRecipe(curChoiceCategory, idx);
    }
    public void CategoryBtnClick(ECraftableItemType eCraftType)
    {
        if (_workbench == null)
        {
            UDebug.Print($"워크밴치를 못찾고 있습니다.", LogType.Assert);
            return;
        }

        _currentChoiceCategory = eCraftType;

        List<RecipeSO> curChoiceRecipeList = _workbench.SelectCategory(_currentChoiceCategory);

        ButtonListClear(_recipeBtnPage);


        for (int i = 0; i < curChoiceRecipeList.Count; i++)
        {
            CraftRecipeBtn tempCraftRecipeBtn = Instantiate(_craftRecipeBtnPrefab, _recipeBtnPage);
            tempCraftRecipeBtn.SetInfo(curChoiceRecipeList[i].Icon, curChoiceRecipeList[i].MakeTargetName, this, i);

            tempCraftRecipeBtn.OnRecipeButtonClick -= RecipeButtonHandler;
            tempCraftRecipeBtn.OnRecipeButtonClick += RecipeButtonHandler;
        }

    }
    public void SetToggleUI()
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
        CategoryBtnClick(ECraftableItemType.Axe);
        RecipeBtnClick(_currentChoiceCategory, 0);
        gameObject.SetActive(true);
        EscManager.Ins.Enter(this);
    }
    public virtual void CloseUI()
    {
        EscManager.Ins.Exit(this);
        gameObject.SetActive(false);
    }

    // ★ IEscClosable 구현 — ESC가 눌렸을 때 EscManager가 직접 호출
    public void CloseUi()
    {
        _isOpen = !_isOpen;
        CloseUI();
    }

    
    public void RecipeButtonHandler(int idx)
    {
        _lastestChoicedBtnIdx = idx;
        RecipeBtnClick(_currentChoiceCategory, _lastestChoicedBtnIdx);
    }
    public void ConfirmButtonHandler()
    {
        _workbench.MakeItem();
        RecipeBtnClick(_currentChoiceCategory, _lastestChoicedBtnIdx);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void SetInfoPage(WorkbenchReturnStruct[] curRecipeRequire, bool canMake)
    {
        ButtonListClear(_requireItemPage);

        for (int i = 0; i < curRecipeRequire.Length; i++)
        {
            CraftRequireItemList tempCraftRequireItemList = Instantiate(_craftRequireItemListPrefab, _requireItemPage);
            tempCraftRequireItemList.SetInfo(curRecipeRequire[i]);
        }
        ShowButton(canMake);
    }
    private void ShowButton(bool canMake)
    {
        _confirmBtn.gameObject.SetActive(canMake);
        _cantBtn.gameObject.SetActive(!canMake);
    }
    private void ButtonListClear(Transform buttonClearTr)
    {
        for(int i= buttonClearTr.childCount-1; i>=0; i--)
        {
            Destroy(buttonClearTr.GetChild(i).gameObject);
        }
    }
    //public void CloseUi()
    //{
    //    CloseUI();
    //}
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    // ★ SetActive(true)  → EscManager 자동 등록
    private void OnEnable()
    {
        EscManager.Ins.Enter(this);
    }

    // ★ SetActive(false) → EscManager 자동 해제
    private void OnDisable()
    {
        EscManager.Ins.Exit(this);
    }

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion



}
