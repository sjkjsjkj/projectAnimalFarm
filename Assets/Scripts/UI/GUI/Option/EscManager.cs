using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Esc키를 인식하여 Ui를 하나 종료하거나 게임 메뉴를 엽니다.
/// </summary>
public class EscManager : GlobalSingleton<EscManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private List<IEscClosable> _activeUis = new(); // 활성화된 Ui 목록
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public event Action OnEnableEscPressed;

    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }

    public void Enter(IEscClosable comp)
    {
        if (_activeUis.Contains(comp))
        {
            UDebug.Print($"{comp}는 이미 EscManager에 들어왔습니다.", LogType.Warning);
            return;
        }
        _activeUis.Add(comp);
    }

    public void Exit(IEscClosable comp)
    {
        if (!_activeUis.Contains(comp))
        {
            UDebug.Print($"{comp}는 EscManager에 들어와있지 않습니다.", LogType.Warning);
            return;
        }
        _activeUis.Remove(comp);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void EscHandle(OnPlayerEsc ctx)
    {
        // 창 닫기
        if (_activeUis.Count > 0)
        {
            int topIndex = _activeUis.Count - 1; // 최상단 번호
            IEscClosable topUi = _activeUis[topIndex]; // 최상단 Ui
            _activeUis.RemoveAt(topIndex);
            topUi.CloseUi();
        }
        // 게임 메뉴 호출
        else if(GameManager.Ins.Scene != EScene.Title)
        {
            OnEnableEscPressed?.Invoke();
        }
    }

    private void OnEnable()
    {
        EventBus<OnPlayerEsc>.Subscribe(EscHandle);
    }

    private void OnDisable()
    {
        EventBus<OnPlayerEsc>.Unsubscribe(EscHandle);
    }
    #endregion
}
