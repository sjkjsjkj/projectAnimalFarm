using UnityEngine;

/// <summary>
/// 낚시 / 채집 이벤트를
/// 1. 피드백 메시지 이벤트로 변환하고
/// 2. 사운드를 재생하는 브릿지 컴포넌트
/// 
/// - 기존 UIFeedbackMessageStackPresenter를 직접 참조하지 않습니다.
/// - OnFeedbackMessageRequested.Publish(...)만 호출하면 UI는 자동 처리됩니다.
/// - 메시지 최대 길이(30자)를 넘기면 기존 시스템에서 무시되므로,
///   여기서 방어적으로 잘라서 발행합니다.
/// </summary>
public class InteractionFeedbackRelay : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("사운드 출력 위치")]
    [SerializeField] private Transform _soundTarget;

    [Header("메시지 표시 시간")]
    [SerializeField] private float _defaultDuration = 1.5f;
    [SerializeField] private float _startDuration = 1.0f;
    [SerializeField] private float _successDuration = 1.5f;
    [SerializeField] private float _failDuration = 1.5f;

    [Header("낚시 메시지")]
    [SerializeField] private string _fishingStartMessage = "낚시 중...";
    [SerializeField] private string _fishingSuccessMessage = "물고기를 획득했습니다.";
    [SerializeField] private string _fishingFailMessage = "낚시에 실패했습니다.";
    [SerializeField] private string _fishingCancelMessage = "낚시를 취소했습니다.";

    [Header("채집 메시지")]
    [SerializeField] private string _collectStartMessage = "채집 중...";
    [SerializeField] private string _collectSuccessMessage = "아이템을 획득했습니다.";
    [SerializeField] private string _collectFailMessage = "채집에 실패했습니다.";
    [SerializeField] private string _collectCancelMessage = "채집을 취소했습니다.";

    [Header("낚시 사운드")]
    [SerializeField]
    private string[] _fishingStartSoundIds =
    {
        Id.Sfx_Player_FishingWater_1,
        Id.Sfx_Player_FishingWater_2,
        Id.Sfx_Player_FishingWater_3,
        Id.Sfx_Player_FishingWater_4
    };

    [SerializeField]
    private string[] _fishingSuccessSoundIds =
    {
        Id.Sfx_Other_Success_1,
        Id.Sfx_Other_Success_2
    };

    [SerializeField]
    private string[] _fishingFailSoundIds =
    {
        Id.Sfx_Ui_No_1,
        Id.Sfx_Ui_No_2
    };

    [SerializeField]
    private string[] _fishingCancelSoundIds =
    {
        Id.Sfx_Ui_Click_1
    };

    [Header("채집 사운드")]
    [SerializeField]
    private string[] _collectStartSoundIds =
    {
        Id.Sfx_Player_PlowField_01,
        Id.Sfx_Player_PlowField_02,
        Id.Sfx_Player_PlowField_03,
        Id.Sfx_Player_PlowField_04
    };

    [SerializeField]
    private string[] _collectSuccessSoundIds =
    {
        Id.Sfx_Other_Success_1,
        Id.Sfx_Other_Success_2
    };

    [SerializeField]
    private string[] _collectFailSoundIds =
    {
        Id.Sfx_Ui_No_1,
        Id.Sfx_Ui_No_2
    };

    [SerializeField]
    private string[] _collectCancelSoundIds =
    {
        Id.Sfx_Ui_Click_1
    };

    [Header("로그")]
    [SerializeField] private bool _logEnabled = false;
    #endregion

    #region ─────────────────────────▶ Unity 생명주기 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        if (_soundTarget == null)
        {
            _soundTarget = transform;
        }
    }
    #endregion

    #region ─────────────────────────▶ 외부 메시지 함수 ◀─────────────────────────
    /// <summary>
    /// UnityEvent(string) 연결용
    /// 기본적으로 일반 안내/주의성 메시지로 출력
    /// </summary>
    public void ShowWarningFeedback(string message)
    {
        PublishFeedback(message, EFeedbackMessageType.Warning, _defaultDuration);
    }

    /// <summary>
    /// UnityEvent(string) 연결용
    /// 실패 메시지 출력
    /// </summary>
    public void ShowFailureFeedback(string message)
    {
        PublishFeedback(message, EFeedbackMessageType.Failure, _failDuration);
    }

    /// <summary>
    /// UnityEvent(string) 연결용
    /// 성공 메시지 출력
    /// </summary>
    public void ShowSuccessFeedback(string message)
    {
        PublishFeedback(message, EFeedbackMessageType.Success, _successDuration);
    }
    #endregion

    #region ─────────────────────────▶ 낚시 이벤트 ◀─────────────────────────
    public void OnFishingStarted()
    {
        PublishFeedback(_fishingStartMessage, EFeedbackMessageType.Warning, _startDuration);
        PlayRandom(_fishingStartSoundIds);
    }

    public void OnFishingSucceeded()
    {
        PublishFeedback(_fishingSuccessMessage, EFeedbackMessageType.Success, _successDuration);
        PlayRandom(_fishingSuccessSoundIds);
    }

    public void OnFishingFailed()
    {
        PublishFeedback(_fishingFailMessage, EFeedbackMessageType.Failure, _failDuration);
        PlayRandom(_fishingFailSoundIds);
    }

    public void OnFishingCanceled()
    {
        PublishFeedback(_fishingCancelMessage, EFeedbackMessageType.Warning, _defaultDuration);
        PlayRandom(_fishingCancelSoundIds);
    }
    #endregion

    #region ─────────────────────────▶ 채집 이벤트 ◀─────────────────────────
    public void OnCollectStarted()
    {
        PublishFeedback(_collectStartMessage, EFeedbackMessageType.Warning, _startDuration);
        PlayRandom(_collectStartSoundIds);
    }

    public void OnCollected()
    {
        PublishFeedback(_collectSuccessMessage, EFeedbackMessageType.Success, _successDuration);
        PlayRandom(_collectSuccessSoundIds);
    }

    public void OnCollectFailed()
    {
        PublishFeedback(_collectFailMessage, EFeedbackMessageType.Failure, _failDuration);
        PlayRandom(_collectFailSoundIds);
    }

    public void OnCollectCanceled()
    {
        PublishFeedback(_collectCancelMessage, EFeedbackMessageType.Warning, _defaultDuration);
        PlayRandom(_collectCancelSoundIds);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void PublishFeedback(string message, EFeedbackMessageType messageType, float duration)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (messageType == EFeedbackMessageType.None)
        {
            return;
        }

        string safeMessage = message.Trim();

        if (safeMessage.Length <= 0)
        {
            return;
        }

        // 기존 OnFeedbackMessageRequested는 30자 초과 메시지를 무시하므로
        // 브릿지 쪽에서 방어적으로 잘라줍니다.
        if (safeMessage.Length > 30)
        {
            safeMessage = safeMessage.Substring(0, 30);
        }

        float safeDuration = Mathf.Max(0f, duration);

        OnFeedbackMessageRequested.Publish(safeMessage, messageType, safeDuration);

        if (_logEnabled)
        {
            Debug.Log($"[InteractionFeedbackRelay] Feedback Published | Type: {messageType}, Message: {safeMessage}, Duration: {safeDuration}");
        }
    }

    private void PlayRandom(string[] soundIds)
    {
        if (soundIds == null || soundIds.Length == 0)
        {
            return;
        }

        int validCount = 0;
        for (int i = 0; i < soundIds.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(soundIds[i]))
            {
                validCount++;
            }
        }

        if (validCount <= 0)
        {
            return;
        }

        int targetIndex = Random.Range(0, validCount);
        int currentIndex = 0;

        for (int i = 0; i < soundIds.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(soundIds[i]))
            {
                continue;
            }

            if (currentIndex == targetIndex)
            {
                if (_soundTarget != null)
                {
                    USound.PlaySfx(soundIds[i], _soundTarget);
                }
                else
                {
                    USound.PlaySfx(soundIds[i]);
                }

                if (_logEnabled)
                {
                    Debug.Log($"[InteractionFeedbackRelay] Sound Played: {soundIds[i]}");
                }

                return;
            }

            currentIndex++;
        }
    }
    #endregion
}
