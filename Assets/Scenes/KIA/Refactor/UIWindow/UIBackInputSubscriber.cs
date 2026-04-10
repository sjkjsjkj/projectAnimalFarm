using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// UI 뒤로가기 입력을 감지하여 공통 이벤트를 발행하는 컴포넌트입니다.
/// 현재는 Esc 키를 기준으로 동작합니다.
/// </summary>
public class UIBackInputSubscriber : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    /// <summary>
    /// 뒤로가기 입력 사용 여부입니다.
    /// </summary>
    [Header("뒤로가기 입력 설정")]
    [SerializeField] private bool _enableInput = true;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// Esc 입력을 검사하여 뒤로가기 이벤트를 발행합니다.
    /// </summary>
    private void HandleBackInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame == false)
        {
            return;
        }

        OnUIBackRequested.Publish();
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (_enableInput == false)
        {
            return;
        }

        HandleBackInput();
    }
    #endregion
}
