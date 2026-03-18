using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 표지판 오브젝트가 사용하는 상호작용 클래스입니다.
/// </summary>
public class ExSignInteractable : ExInteractableBase
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("필수 요소 등록")]
    [TextArea(2, 6)]
    [SerializeField] private string _message = "Sign Object";
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    protected override void OnInteract(ExInteractor interactor)
    {
        UDebug.Print($"[Sign] {_message}");
    }
    #endregion
}
