using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 피드백 메시지 스택 UI가 정상 동작하는지 테스트하기 위한 디버그 입력 컴포넌트입니다.
/// 숫자 키 입력으로 각 종류의 피드백 메시지를 발행합니다.
/// </summary>
public class UIFeedbackMessageDebugTester : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    /// <summary>
    /// 테스트 입력 사용 여부입니다.
    /// </summary>
    [Header("테스트 입력 설정")]
    [SerializeField] private bool _enableTestInput = true;

    /// <summary>
    /// 피드백 메시지 기본 유지 시간입니다.
    /// </summary>
    [SerializeField] private float _defaultDuration = 1.5f;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 성공 메시지 테스트 이벤트를 발행합니다.
    /// </summary>
    private void PublishSuccessMessage()
    {
        OnFeedbackMessageRequested.Publish("아이템을 획득했습니다.", EFeedbackMessageType.Success, _defaultDuration);
    }

    /// <summary>
    /// 경고 메시지 테스트 이벤트를 발행합니다.
    /// </summary>
    private void PublishWarningMessage()
    {
        OnFeedbackMessageRequested.Publish("인벤토리가 거의 가득 찼습니다.", EFeedbackMessageType.Warning, _defaultDuration);
    }

    /// <summary>
    /// 실패 메시지 테스트 이벤트를 발행합니다.
    /// </summary>
    private void PublishFailureMessage()
    {
        OnFeedbackMessageRequested.Publish("아이템을 이동할 수 없습니다.", EFeedbackMessageType.Failure, _defaultDuration);
    }

    /// <summary>
    /// 긴 문장 표시 테스트 이벤트를 발행합니다.
    /// </summary>
    private void PublishLongMessage()
    {
        OnFeedbackMessageRequested.Publish("ABCDEFGHIJKLMNOPYZABCDEFGHIJ", EFeedbackMessageType.Warning, 2.5f);
    }

    /// <summary>
    /// 키보드 입력을 검사하여 테스트 메시지를 발행합니다.
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            PublishSuccessMessage();
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            PublishWarningMessage();
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            PublishFailureMessage();
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            PublishLongMessage();
        }
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (_enableTestInput == false)
        {
            return;
        }

        HandleKeyboardInput();
    }
    #endregion
}
