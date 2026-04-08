using TMPro;
using UnityEngine;

/// <summary>
/// 피드백 메시지 아이템 1개의 실제 표현을 담당하는 뷰 컴포넌트입니다.
/// 텍스트, 아이콘, 알파 값을 제어합니다.
/// </summary>
public class UIFeedbackMessageItemView : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("피드백 메시지 아이템 참조")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [SerializeField] private TMP_Text _messageText;         // 문자열 출력 텍스트 참조 
    [SerializeField] private GameObject _successIconObject; // 성공아이콘 오브젝트 참조 
    [SerializeField] private GameObject _warningIconObject; // 경고아이콘 오브젝트 참조
    [SerializeField] private GameObject _failureIconObject; //실패 아이콘 오브젝트 참조
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 모든 아이콘을 비활성화합니다.
    /// </summary>
    private void HideAllIcons()
    {
        if (_successIconObject != null)
        {
            _successIconObject.SetActive(false);
        }

        if (_warningIconObject != null)
        {
            _warningIconObject.SetActive(false);
        }

        if (_failureIconObject != null)
        {
            _failureIconObject.SetActive(false);
        }
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 메시지 아이템 내용을 초기화합니다.
    /// </summary>
    /// <param name="message">출력할 메시지</param>
    /// <param name="messageType">메시지 타입</param>
    public void Initialize(string message, EFeedbackMessageType messageType)
    {
        SetMessage(message);
        SetMessageType(messageType);
        SetAlpha(0f);                   // 초기 알파를 0으로 설정
    }

    /// <summary>
    /// 메시지 문자열을 설정합니다.
    /// </summary>
    /// <param name="message">출력할 메시지</param>
    public void SetMessage(string message)
    {
        if (_messageText == null)
        {
            return;
        }

        _messageText.text = message;
    }

    /// <summary>
    /// 메시지 타입에 맞는 아이콘만 표시합니다.
    /// </summary>
    /// <param name="messageType">표시할 메시지 타입</param>
    public void SetMessageType(EFeedbackMessageType messageType)
    {
        HideAllIcons();

        switch (messageType)
        {
            case EFeedbackMessageType.Success:
                if (_successIconObject != null)
                {
                    _successIconObject.SetActive(true);
                }
                break;

            case EFeedbackMessageType.Warning:
                if (_warningIconObject != null)
                {
                    _warningIconObject.SetActive(true);
                }
                break;

            case EFeedbackMessageType.Failure:
                if (_failureIconObject != null)
                {
                    _failureIconObject.SetActive(true);
                }
                break;
        }
    }

    /// <summary>
    /// 메시지 아이템의 알파 값을 설정합니다.
    /// </summary>
    /// <param name="alpha">0~1 범위의 알파 값</param>
    public void SetAlpha(float alpha)
    {
        if (_canvasGroup == null)
        {
            return;
        }

        _canvasGroup.alpha = Mathf.Clamp01(alpha);
        _canvasGroup.interactable = false;              // 피드백 메시지는 상호작용하지 않음
        _canvasGroup.blocksRaycasts = false;            // 하위 UI 클릭을 막지 않음
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 인스펙터 연결 편의를 위해 자식 오브젝트를 자동 탐색합니다.
    /// </summary>
    protected void Reset()
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        if (_messageText == null)
        {
            Transform messageText = transform.Find("MessageText");
            if (messageText != null)
            {
                _messageText = messageText.GetComponent<TMP_Text>();
            }
        }

        if (_successIconObject == null)
        {
            Transform successIcon = transform.Find("SuccessIcon");
            if (successIcon != null)
            {
                _successIconObject = successIcon.gameObject;
            }
        }

        if (_warningIconObject == null)
        {
            Transform warningIcon = transform.Find("WarningIcon");
            if (warningIcon != null)
            {
                _warningIconObject = warningIcon.gameObject;
            }
        }

        if (_failureIconObject == null)
        {
            Transform failureIcon = transform.Find("FailureIcon");
            if (failureIcon != null)
            {
                _failureIconObject = failureIcon.gameObject;
            }
        }
    }
    #endregion
}
