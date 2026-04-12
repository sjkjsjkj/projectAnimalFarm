using System.Collections;
using UnityEngine;

/// <summary>
/// 필드에서 제작대와 인터랙션할 때 필요한 오브젝트입니다.
/// </summary>
public class WorkbenchIteractionObject : BaseMono, IInteractable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("제작대 UI")]
    [SerializeField] private WorkbenchUI _workUI;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool CanInteract(GameObject player)
    {
        return true;
    }

    public string GetMessage()
    {
        return "제작대 UI 인터랙션";
    }

    public void Interact(GameObject player)
    {
        UDebug.Print("?");
        if(_workUI == null)
        {
            UDebug.Print("12?");
        }
        _workUI.SetToggleUI();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private IEnumerator CoWaitRootLoading()
    {
        while(true)
        {
            if(WorkbenchManager.Ins.IsSettingFinish)
            {
                break;
            }
            yield return null;
        }
        _workUI = WorkbenchManager.Ins.WorkbenchUI;
        _workUI.gameObject.SetActive(false);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        StartCoroutine(CoWaitRootLoading());
       
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion

}
