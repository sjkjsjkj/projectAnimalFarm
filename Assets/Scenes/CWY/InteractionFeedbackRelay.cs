using UnityEngine;

/// <summary>
/// 낚시 / 채집 / 채굴 이벤트를
/// 1. 피드백 메시지 이벤트로 변환하고
/// 2. 사운드를 재생하는 브릿지 컴포넌트
///
/// - UIFeedbackMessageStackPresenter를 직접 참조하지 않습니다.
/// - OnFeedbackMessageRequested.Publish(...)만 호출하면 UI는 자동 처리됩니다.
/// - 메시지 최대 길이(30자)를 넘기면 기존 시스템에서 무시되므로,
///   여기서 방어적으로 잘라서 발행합니다.
/// - 모든 상태 메시지를 이 컴포넌트에서 중앙 관리합니다.
/// </summary>
public class InteractionFeedbackRelay : BaseMono
{
    public enum EInteractionFeedbackKey
    {
        None = 0,

        // 공용
        InventoryFull,

        // 낚시
        FishingStart,
        FishingSuccess,
        FishingFail,
        FishingCancel,
        FishingNoBait,
        FishingNoRod,
        FishingBaitInsufficient,
        FishingToolRarityLow,
        FishingBaitRarityLow,

        // 채집
        CollectStart,
        CollectSuccess,
        CollectFail,
        CollectCancel,
        CollectNeedPickaxe,
        CollectNeedShovel,
        CollectNeedSickle,
        CollectNeedAxe,
        CollectToolRarityLow,

        // 채굴
        MiningNoPickaxe,
        MiningFail,
        MiningToolRarityLow,
    }

    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("사운드 출력 위치")]
    [SerializeField] private Transform _soundTarget;

    [Header("메시지 표시 시간")]
    [SerializeField] private float _defaultDuration = 1.5f;
    [SerializeField] private float _startDuration = 1.0f;
    [SerializeField] private float _successDuration = 1.5f;
    [SerializeField] private float _failDuration = 1.5f;

    [Header("공용 메시지")]
    [SerializeField] private string _inventoryFullMessage = "인벤토리가 가득 찼습니다.";

    [Header("낚시 메시지")]
    [SerializeField] private string _fishingStartMessage = "낚시 중...";
    [SerializeField] private string _fishingSuccessMessage = "물고기를 획득했습니다.";
    [SerializeField] private string _fishingFailMessage = "낚시에 실패했습니다.";
    [SerializeField] private string _fishingCancelMessage = "낚시를 취소했습니다.";
    [SerializeField] private string _fishingNoBaitMessage = "미끼가 없습니다.";
    [SerializeField] private string _fishingNoRodMessage = "낚싯대가 없습니다.";
    [SerializeField] private string _fishingBaitInsufficientMessage = "미끼가 부족합니다.";
    [SerializeField] private string _fishingToolRarityLowMessage = "아이템 등급이 낮습니다.";
    [SerializeField] private string _fishingBaitRarityLowMessage = "미끼의 등급이 낮습니다.";

    [Header("채집 메시지")]
    [SerializeField] private string _collectStartMessage = "채집 중...";
    [SerializeField] private string _collectSuccessMessage = "아이템을 획득했습니다.";
    [SerializeField] private string _collectFailMessage = "채집에 실패하였습니다.";
    [SerializeField] private string _collectCancelMessage = "채집을 취소했습니다.";
    [SerializeField] private string _collectNeedPickaxeMessage = "곡괭이가 필요합니다.";
    [SerializeField] private string _collectNeedShovelMessage = "삽이 없습니다.";
    [SerializeField] private string _collectNeedSickleMessage = "낫이 필요합니다.";
    [SerializeField] private string _collectNeedAxeMessage = "도끼가 필요합니다.";
    [SerializeField] private string _collectToolRarityLowMessage = "도구 등급이 낮습니다.";

    [Header("채굴 메시지")]
    [SerializeField] private string _miningNoPickaxeMessage = "곡괭이가 없습니다.";
    [SerializeField] private string _miningFailMessage = "채광에 실패하였습니다.";
    [SerializeField] private string _miningToolRarityLowMessage = "도구 등급이 낮습니다.";

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

    [Header("채굴 사운드")]
    [SerializeField]
    private string[] _miningFailSoundIds =
    {
        Id.Sfx_Ui_No_1,
        Id.Sfx_Ui_No_2
    };

    [SerializeField]
    private string[] _miningWarningSoundIds =
    {
        Id.Sfx_Ui_No_1,
        Id.Sfx_Ui_No_2
    };

    [Header("상태 실패 공용 사운드")]
    [SerializeField]
    private string[] _warningSoundIds =
    {
        Id.Sfx_Ui_No_1,
        Id.Sfx_Ui_No_2
    };

    [SerializeField]
    private string[] _failureSoundIds =
    {
        Id.Sfx_Ui_No_1,
        Id.Sfx_Ui_No_2
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
        PlayRandom(_warningSoundIds);
    }

    /// <summary>
    /// UnityEvent(string) 연결용
    /// 실패 메시지 출력
    /// </summary>
    public void ShowFailureFeedback(string message)
    {
        PublishFeedback(message, EFeedbackMessageType.Failure, _failDuration);
        PlayRandom(_failureSoundIds);
    }

    /// <summary>
    /// UnityEvent(string) 연결용
    /// 성공 메시지 출력
    /// </summary>
    public void ShowSuccessFeedback(string message)
    {
        PublishFeedback(message, EFeedbackMessageType.Success, _successDuration);
    }

    /// <summary>
    /// 코드에서 상태 키만 넘겨서 출력할 때 사용
    /// </summary>
    public void ShowFeedback(EInteractionFeedbackKey key)
    {
        if (!TryResolvePreset(key, out string message, out EFeedbackMessageType messageType, out float duration, out string[] soundIds))
        {
            return;
        }

        PublishFeedback(message, messageType, duration);
        PlayRandom(soundIds);
    }
    #endregion

    #region ─────────────────────────▶ 낚시 이벤트 ◀─────────────────────────
    public void OnFishingStarted() => ShowFeedback(EInteractionFeedbackKey.FishingStart);
    public void OnFishingSucceeded() => ShowFeedback(EInteractionFeedbackKey.FishingSuccess);
    public void OnFishingFailed() => ShowFeedback(EInteractionFeedbackKey.FishingFail);
    public void OnFishingCanceled() => ShowFeedback(EInteractionFeedbackKey.FishingCancel);

    public void OnFishingNoBait() => ShowFeedback(EInteractionFeedbackKey.FishingNoBait);
    public void OnFishingNoRod() => ShowFeedback(EInteractionFeedbackKey.FishingNoRod);
    public void OnFishingBaitInsufficient() => ShowFeedback(EInteractionFeedbackKey.FishingBaitInsufficient);
    public void OnFishingToolRarityLow() => ShowFeedback(EInteractionFeedbackKey.FishingToolRarityLow);
    public void OnFishingBaitRarityLow() => ShowFeedback(EInteractionFeedbackKey.FishingBaitRarityLow);
    #endregion

    #region ─────────────────────────▶ 채집 이벤트 ◀─────────────────────────
    public void OnCollectStarted() => ShowFeedback(EInteractionFeedbackKey.CollectStart);
    public void OnCollected() => ShowFeedback(EInteractionFeedbackKey.CollectSuccess);
    public void OnCollectFailed() => ShowFeedback(EInteractionFeedbackKey.CollectFail);
    public void OnCollectCanceled() => ShowFeedback(EInteractionFeedbackKey.CollectCancel);

    public void OnCollectNeedPickaxe() => ShowFeedback(EInteractionFeedbackKey.CollectNeedPickaxe);
    public void OnCollectNeedShovel() => ShowFeedback(EInteractionFeedbackKey.CollectNeedShovel);
    public void OnCollectNeedSickle() => ShowFeedback(EInteractionFeedbackKey.CollectNeedSickle);
    public void OnCollectNeedAxe() => ShowFeedback(EInteractionFeedbackKey.CollectNeedAxe);
    public void OnCollectToolRarityLow() => ShowFeedback(EInteractionFeedbackKey.CollectToolRarityLow);
    #endregion

    #region ─────────────────────────▶ 채굴 이벤트 ◀─────────────────────────
    public void OnMiningNoPickaxe() => ShowFeedback(EInteractionFeedbackKey.MiningNoPickaxe);
    public void OnMiningFailed() => ShowFeedback(EInteractionFeedbackKey.MiningFail);
    public void OnMiningToolRarityLow() => ShowFeedback(EInteractionFeedbackKey.MiningToolRarityLow);
    #endregion

    #region ─────────────────────────▶ 공용 이벤트 ◀─────────────────────────
    public void OnInventoryFull() => ShowFeedback(EInteractionFeedbackKey.InventoryFull);
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private bool TryResolvePreset(
        EInteractionFeedbackKey key,
        out string message,
        out EFeedbackMessageType messageType,
        out float duration,
        out string[] soundIds)
    {
        message = string.Empty;
        messageType = EFeedbackMessageType.None;
        duration = _defaultDuration;
        soundIds = null;

        switch (key)
        {
            case EInteractionFeedbackKey.InventoryFull:
                message = _inventoryFullMessage;
                messageType = EFeedbackMessageType.Failure;
                duration = _failDuration;
                soundIds = _failureSoundIds;
                return true;

            case EInteractionFeedbackKey.FishingStart:
                message = _fishingStartMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _startDuration;
                soundIds = _fishingStartSoundIds;
                return true;

            case EInteractionFeedbackKey.FishingSuccess:
                message = _fishingSuccessMessage;
                messageType = EFeedbackMessageType.Success;
                duration = _successDuration;
                soundIds = _fishingSuccessSoundIds;
                return true;

            case EInteractionFeedbackKey.FishingFail:
                message = _fishingFailMessage;
                messageType = EFeedbackMessageType.Failure;
                duration = _failDuration;
                soundIds = _fishingFailSoundIds;
                return true;

            case EInteractionFeedbackKey.FishingCancel:
                message = _fishingCancelMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _fishingCancelSoundIds;
                return true;

            case EInteractionFeedbackKey.FishingNoBait:
                message = _fishingNoBaitMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _warningSoundIds;
                return true;

            case EInteractionFeedbackKey.FishingNoRod:
                message = _fishingNoRodMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _warningSoundIds;
                return true;

            case EInteractionFeedbackKey.FishingBaitInsufficient:
                message = _fishingBaitInsufficientMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _warningSoundIds;
                return true;

            case EInteractionFeedbackKey.FishingToolRarityLow:
                message = _fishingToolRarityLowMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _warningSoundIds;
                return true;

            case EInteractionFeedbackKey.FishingBaitRarityLow:
                message = _fishingBaitRarityLowMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _warningSoundIds;
                return true;

            case EInteractionFeedbackKey.CollectStart:
                message = _collectStartMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _startDuration;
                soundIds = _collectStartSoundIds;
                return true;

            case EInteractionFeedbackKey.CollectSuccess:
                message = _collectSuccessMessage;
                messageType = EFeedbackMessageType.Success;
                duration = _successDuration;
                soundIds = _collectSuccessSoundIds;
                return true;

            case EInteractionFeedbackKey.CollectFail:
                message = _collectFailMessage;
                messageType = EFeedbackMessageType.Failure;
                duration = _failDuration;
                soundIds = _collectFailSoundIds;
                return true;

            case EInteractionFeedbackKey.CollectCancel:
                message = _collectCancelMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _collectCancelSoundIds;
                return true;

            case EInteractionFeedbackKey.CollectNeedPickaxe:
                message = _collectNeedPickaxeMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _warningSoundIds;
                return true;

            case EInteractionFeedbackKey.CollectNeedShovel:
                message = _collectNeedShovelMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _warningSoundIds;
                return true;

            case EInteractionFeedbackKey.CollectNeedSickle:
                message = _collectNeedSickleMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _warningSoundIds;
                return true;

            case EInteractionFeedbackKey.CollectNeedAxe:
                message = _collectNeedAxeMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _warningSoundIds;
                return true;

            case EInteractionFeedbackKey.CollectToolRarityLow:
                message = _collectToolRarityLowMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _warningSoundIds;
                return true;

            case EInteractionFeedbackKey.MiningNoPickaxe:
                message = _miningNoPickaxeMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _miningWarningSoundIds;
                return true;

            case EInteractionFeedbackKey.MiningFail:
                message = _miningFailMessage;
                messageType = EFeedbackMessageType.Failure;
                duration = _failDuration;
                soundIds = _miningFailSoundIds;
                return true;

            case EInteractionFeedbackKey.MiningToolRarityLow:
                message = _miningToolRarityLowMessage;
                messageType = EFeedbackMessageType.Warning;
                duration = _defaultDuration;
                soundIds = _miningWarningSoundIds;
                return true;
        }

        return false;
    }

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
        // 릴레이 쪽에서 방어적으로 잘라줍니다.
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
