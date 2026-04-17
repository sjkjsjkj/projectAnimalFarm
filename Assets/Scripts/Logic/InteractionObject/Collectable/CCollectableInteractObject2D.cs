using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 채집 가능한 오브젝트
///
/// 수정 핵심
/// 1. 버튼 채집은 가까운 거리 안에서만 가능
/// 2. ButtonOnly일 때 자동 채집이 절대 발생하지 않도록 보강
/// 3. AutoOnly / AutoOrButton도 자동 채집 거리 제한 추가
/// 4. 수동 채집 도중 이동 시 취소
/// 5. 보상 아이템이 OreItem이면 채굴 컨텍스트로 처리
/// 6. InteractionFeedbackRelay를 통해 채집 / 채굴 / 인벤토리 full 상태 메시지 출력
/// 7. 플레이어가 가까이 왔을 때만 채집/채광 가능 아이콘 표시
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CCollectableInteractObject2D : BaseMono, IInteractable
{
    private const float DEFAULT_FEEDBACK_DURATION = 1.5f;
    private const float START_FEEDBACK_DURATION = 1.0f;
    private const float SUCCESS_FEEDBACK_DURATION = 1.5f;
    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }

    public enum ECollectMode
    {
        AutoOnly = 0,
        ButtonOnly = 1,
        AutoOrButton = 2,
    }

    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("지급 아이템 정보")]
    [Tooltip("실제로 플레이어 인벤토리에 들어갈 아이템 ID")]
    [SerializeField] private string _itemId;

    [Tooltip("한 번 획득 시 지급할 수량")]
    [SerializeField] private int _amount = 1;

    [Tooltip("UI에 보여줄 이름. 비어 있으면 itemId를 대신 사용")]
    [SerializeField] private string _displayName = "";

    [Header("획득 방식")]
    [Tooltip("자동 / 수동 / 둘 다 가능한지 설정")]
    [SerializeField] private ECollectMode _collectMode = ECollectMode.ButtonOnly;

    [Tooltip("AutoOnly / AutoOrButton일 때 자동 채집까지 걸리는 시간")]
    [SerializeField] private float _autoCollectDelay = 0.15f;

    [Header("거리 제한")]
    [Tooltip("버튼 채집 가능 최대 거리. 이 거리보다 멀면 버튼을 눌러도 채집 불가")]
    [SerializeField] private float _manualCollectMaxDistance = 1.0f;

    [Tooltip("자동 채집 가능 최대 거리. 이 거리보다 멀면 자동 채집 불가")]
    [SerializeField] private float _autoCollectMaxDistance = 0.8f;

    [Header("채집 가능 아이콘")]
    [Tooltip("플레이어가 가까이 왔을 때만 켜둘 표시용 오브젝트")]
    [SerializeField] private GameObject _canCollectIndicatorObject;

    [Tooltip("채집/채광 진행 중에는 아이콘을 숨길지 여부")]
    [SerializeField] private bool _hideIndicatorWhileCollecting = true;

    [Header("대표 도구 요구 조건")]
    [Tooltip("체크하면 플레이어 인벤토리의 대표 도구 기준으로 채집 가능 여부를 검사합니다.")]
    [SerializeField] private bool _requireEquippedTool = true;

    [Tooltip("직접 지정하면 이 도구를 요구합니다. None이면 보상 아이템 타입으로 자동 추론합니다.")]
    [SerializeField] private EType _requiredToolType = EType.None;

    [Tooltip("체크하면 보상 아이템의 등급을 최소 요구 도구 등급으로 사용합니다.")]
    [SerializeField] private bool _useRewardItemRarityAsRequiredToolRarity = true;

    [Tooltip("직접 지정할 최소 도구 등급입니다.")]
    [SerializeField] private ERarity _requiredToolMinRarity = ERarity.Basic;

    [Header("수동 채집 연출")]
    [Tooltip("버튼 채집 시 채집 애니메이션 / 딜레이를 사용할지")]
    [SerializeField] private bool _useInteractionMotion = true;

    [Tooltip("버튼 채집 시 실제 보상 지급까지 대기 시간")]
    [SerializeField] private float _manualCollectDelay = 1.0f;

    [Tooltip("도구 등급이 요구 등급보다 높을수록 상호작용 시간을 줄일지 여부")]
    [SerializeField] private bool _useToolRarityBasedDuration = true;

    [Tooltip("도구 등급이 요구 등급보다 1단계 높을 때마다 감소할 시간")]
    [SerializeField] private float _manualCollectDurationReducePerRarityStep = 0.15f;

    [Tooltip("도구 등급 보정으로 줄어들 수 있는 최소 상호작용 시간")]
    [SerializeField] private float _manualCollectMinDuration = 0.35f;

    [Tooltip("버튼 채집 중 플레이어를 Busy 상태로 묶을지")]
    [SerializeField] private bool _setPlayerBusyDuringManualCollect = true;

    [Tooltip("채집 성공 시 지급할 생활 스킬 경험치")]
    [SerializeField] private int _gatherSkillExp = 0;

    [Header("이동 취소 설정")]
    [Tooltip("이동 입력의 sqrMagnitude가 이 값보다 크면 취소")]
    [SerializeField] private float _moveCancelInputThreshold = 0.0001f;

    [Tooltip("시작 위치와 현재 위치 차이의 sqrMagnitude가 이 값보다 크면 취소")]
    [SerializeField] private float _moveCancelPositionThreshold = 0.0001f;

    [SerializeField] private string _manualCollectCancelMessage = "이동하여 채집이 취소되었습니다.";

    [Header("피드백 릴레이")]
    [SerializeField] private InteractionFeedbackRelay _feedbackRelay;

    [Header("획득 후 처리")]
    [Tooltip("획득 즉시 이 오브젝트를 파괴할지")]
    [SerializeField] private bool _destroyOnCollected = false;

    [Tooltip("획득 후 꺼둘 대상. 연결하면 이 오브젝트 대신 이 대상을 꺼줌")]
    [SerializeField] private GameObject _disableTargetAfterCollected;

    [Header("리스폰 설정")]
    [Tooltip("일정 시간 후 다시 등장하게 할지")]
    [SerializeField] private bool _useRespawn = false;

    [Tooltip("리스폰까지 대기 시간")]
    [SerializeField] private float _respawnDelay = 10f;

    [Header("이벤트")]
    [SerializeField] private UnityEvent _onCollectStarted;
    [SerializeField] private UnityEvent _onCollected;
    [SerializeField] private UnityEvent _onRespawned;

    [Header("나중에 연결할 이벤트")]
    [Tooltip("기존 Inspector 연결 유지용 string 메시지 이벤트")]
    [SerializeField] private StringEvent _onFeedbackMessage;

    [Tooltip("채집 실패 시 호출. 나중에 사운드/VFX 연결 가능")]
    [SerializeField] private UnityEvent _onCollectFailed;

    [Tooltip("채집 취소 시 호출. 나중에 사운드/VFX 연결 가능")]
    [SerializeField] private UnityEvent _onCollectCanceled;


    [Header("로컬 폴백 사운드")]
    [SerializeField] private Transform _feedbackSoundTarget;

    [SerializeField]
    private string[] _warningFeedbackSoundIds =
    {
        Id.Sfx_Ui_No_1,
        Id.Sfx_Ui_No_2
    };

    [SerializeField]
    private string[] _failureFeedbackSoundIds =
    {
        Id.Sfx_Ui_No_1,
        Id.Sfx_Ui_No_2
    };

    [SerializeField]
    private string[] _successFeedbackSoundIds =
    {
        Id.Sfx_Other_Success_1,
        Id.Sfx_Other_Success_2
    };

    [SerializeField]
    private string[] _cancelFeedbackSoundIds =
    {
        Id.Sfx_Ui_Click_1
    };

    [Header("로그")]
    [SerializeField] private bool _logEnabled = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly List<CPlayerCollector2D> _nearCollectors = new List<CPlayerCollector2D>();

    private bool _isCollected = false;
    private bool _isProcessing = false;

    private Coroutine _autoCollectRoutine = null;
    private Coroutine _respawnRoutine = null;
    private Coroutine _manualCollectRoutine = null;
    private Collider2D _triggerCollider = null;

    private CPlayerCollector2D _manualCollectCollector = null;
    private Vector2 _manualCollectStartPosition = Vector2.zero;
    private bool _isMoveCancelSubscribed = false;
    private string _pendingLocalCollectedSuccessMessage = string.Empty;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public string RewardItemId => _itemId;
    public bool IsCollectableNow => !_isCollected && !_isProcessing;
    public bool CanManualCollect => _collectMode == ECollectMode.ButtonOnly || _collectMode == ECollectMode.AutoOrButton;
    public bool CanAutoCollect => _collectMode == ECollectMode.AutoOnly || _collectMode == ECollectMode.AutoOrButton;
    #endregion

    #region ─────────────────────────▶ IInteractable 구현 ◀─────────────────────────
    public bool CanInteract(GameObject player)
    {
        if (!CanManualCollect)
        {
            return false;
        }

        if (player == null)
        {
            return false;
        }

        CPlayerCollector2D collector = player.GetComponentInParent<CPlayerCollector2D>();
        if (collector == null)
        {
            collector = player.GetComponent<CPlayerCollector2D>();
        }

        if (collector == null)
        {
            return false;
        }

        if (CanHandleAlreadyCollectingInteraction(collector))
        {
            return true;
        }

        return CanCollectBase(collector, true);
    }

    public void Interact(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        CPlayerCollector2D collector = player.GetComponentInParent<CPlayerCollector2D>();
        if (collector == null)
        {
            collector = player.GetComponent<CPlayerCollector2D>();
        }

        if (collector == null)
        {
            if (_logEnabled)
            {
                Debug.LogWarning($"[CCollectableInteractObject2D] player에서 CPlayerCollector2D를 찾지 못했습니다. object={name}");
            }
            return;
        }

        if (TryHandleAlreadyCollectingInteraction(collector))
        {
            return;
        }

        TryCollect(collector);
    }

    public string GetMessage()
    {
        string targetName = GetTargetDisplayName();
        string actionName = GetActionDisplayName();
        string toolGuide = GetRequiredToolGuideMessage();

        if (string.IsNullOrWhiteSpace(toolGuide))
        {
            return $"{targetName} {actionName}";
        }

        return $"{targetName} {actionName} ({toolGuide})";
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public bool CanCollect(CPlayerCollector2D collector)
    {
        return CanCollectBase(collector, true);
    }

    public bool TryCollect(CPlayerCollector2D collector)
    {
        return TryCollectInternal(collector, true);
    }

    public string GetInteractionMessage(KeyCode key)
    {
        string targetName = GetTargetDisplayName();
        string actionName = GetActionDisplayName();
        string toolGuide = GetRequiredToolGuideMessage();

        if (string.IsNullOrWhiteSpace(toolGuide))
        {
            return $"[{key}] {targetName} {actionName}";
        }

        return $"[{key}] {targetName} {actionName} ({toolGuide})";
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private bool CanCollectBase(CPlayerCollector2D collector, bool isManual)
    {
        if (_isCollected)
        {
            return false;
        }

        if (_isProcessing)
        {
            return false;
        }

        if (collector == null)
        {
            return false;
        }

        if (collector.IsBusy)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_itemId))
        {
            return false;
        }

        if (_amount <= 0)
        {
            return false;
        }

        if (isManual)
        {
            if (!IsCollectorWithinDistance(collector, _manualCollectMaxDistance))
            {
                return false;
            }
        }
        else
        {
            if (!IsCollectorWithinDistance(collector, _autoCollectMaxDistance))
            {
                return false;
            }
        }

        return true;
    }

    private bool CanHandleAlreadyCollectingInteraction(CPlayerCollector2D collector)
    {
        if (collector == null)
        {
            return false;
        }

        if (!CanManualCollect)
        {
            return false;
        }

        if (_isCollected)
        {
            return false;
        }

        if (_manualCollectRoutine == null)
        {
            return false;
        }

        if (_manualCollectCollector != collector)
        {
            return false;
        }

        if (!IsCollectorWithinDistance(collector, _manualCollectMaxDistance))
        {
            return false;
        }

        return true;
    }

    private bool TryHandleAlreadyCollectingInteraction(CPlayerCollector2D collector)
    {
        if (!CanHandleAlreadyCollectingInteraction(collector))
        {
            return false;
        }

        ShowAlreadyCollectingFeedback();
        return true;
    }

    private void ShowAlreadyCollectingFeedback()
    {
        string message = IsMiningTarget()
            ? "이미 채광 중입니다."
            : "이미 채집 중입니다.";

        TryResolveFeedbackRelayIfMissing();

        if (_feedbackRelay != null)
        {
            _feedbackRelay.ShowWarningFeedback(message);
            return;
        }

        NotifyWarningFeedback(message);
    }

    private bool TryCollectInternal(CPlayerCollector2D collector, bool isManualRequest)
    {
        if (collector == null)
        {
            return false;
        }

        if (isManualRequest && TryHandleAlreadyCollectingInteraction(collector))
        {
            return false;
        }

        if (isManualRequest && !CanManualCollect)
        {
            UpdateIndicatorVisibility();
            return false;
        }

        if (!isManualRequest && !CanAutoCollect)
        {
            UpdateIndicatorVisibility();
            return false;
        }

        if (!TryValidateRequiredTool(out EType requiredToolType, out bool isRarityLow, out string toolFailMessage))
        {
            if (_logEnabled)
            {
                Debug.Log($"[CCollectableInteractObject2D] 요구 도구 조건 미충족. object={name}, message={toolFailMessage}");
            }

            if (isManualRequest)
            {
                RaiseToolConditionFailFeedback(requiredToolType, isRarityLow, toolFailMessage);
            }

            UpdateIndicatorVisibility();
            return false;
        }

        if (!CanCollectBase(collector, isManualRequest))
        {
            if (_logEnabled)
            {
                Debug.Log($"[CCollectableInteractObject2D] 거리 또는 상태 조건 미충족. object={name}, manual={isManualRequest}");
            }

            UpdateIndicatorVisibility();
            return false;
        }

        if (isManualRequest && _useInteractionMotion)
        {
            if (!collector.CanStartInteraction())
            {
                if (_logEnabled)
                {
                    Debug.Log($"[CCollectableInteractObject2D] 플레이어가 Busy 상태라 채집 시작 불가. object={name}");
                }

                UpdateIndicatorVisibility();
                return false;
            }

            if (!CanInventoryAcceptReward(collector))
            {
                ShowInventoryFullFeedback();
                UpdateIndicatorVisibility();
                return false;
            }

            StartManualCollectRoutine(collector);
            return true;
        }

        if (!CanInventoryAcceptReward(collector))
        {
            ShowInventoryFullFeedback();
            UpdateIndicatorVisibility();
            return false;
        }

        bool received = TryGiveReward(collector);

        if (!received)
        {
            RaiseCollectFailedFeedback();
            UpdateIndicatorVisibility();
            return false;
        }

        if (_gatherSkillExp > 0)
        {
            collector.TryAddLifeSkillExp(ELifeSkill.Gathering, _gatherSkillExp);
        }

        CompleteCollect();
        return true;
    }

    private void StartManualCollectRoutine(CPlayerCollector2D collector)
    {
        if (_manualCollectRoutine != null)
        {
            return;
        }

        if (!IsCollectorWithinDistance(collector, _manualCollectMaxDistance))
        {
            if (_logEnabled)
            {
                Debug.Log($"[CCollectableInteractObject2D] 수동 채집 시작 실패: 너무 멀리 있음. object={name}");
            }

            UpdateIndicatorVisibility();
            return;
        }

        float manualCollectDuration = GetManualCollectDuration();

        _manualCollectCollector = collector;
        _manualCollectStartPosition = collector.transform.position;

        SubscribeMoveCancel();
        RaiseCollectStartedFeedback();

        if (_hideIndicatorWhileCollecting)
        {
            ApplyIndicatorActive(false);
        }

        if (_logEnabled)
        {
            Debug.Log($"[CCollectableInteractObject2D] 수동 채집 시작. object={name}, duration={manualCollectDuration:0.00}");
        }

        _manualCollectRoutine = StartCoroutine(CoManualCollect(collector, manualCollectDuration));
    }


    private IEnumerator CoManualCollect(CPlayerCollector2D collector, float manualCollectDuration)
    {
        if (_isProcessing)
        {
            yield break;
        }

        if (collector == null)
        {
            ReleaseManualCollectState(null);
            yield break;
        }

        ItemSO rewardItemSo = ResolveRewardItemSO();
        if (rewardItemSo == null)
        {
            if (_logEnabled)
            {
                Debug.LogWarning($"[CCollectableInteractObject2D] 보상 아이템을 찾지 못해 수동 채집을 중단합니다. itemId={_itemId}, object={name}");
            }

            ReleaseManualCollectState(collector);
            RaiseCollectFailedFeedback();
            yield break;
        }

        _isProcessing = true;
        manualCollectDuration = Mathf.Max(0f, manualCollectDuration);

        PublishManualCollectAction(rewardItemSo, manualCollectDuration);

        if (_setPlayerBusyDuringManualCollect)
        {
            collector.SetInteractionBusy(true);
        }

        collector.PlayGatherAnimation();

        if (manualCollectDuration > 0f)
        {
            yield return new WaitForSeconds(manualCollectDuration);
        }

        if (HasCollectorMovedSinceStart(collector))
        {
            CancelManualCollectInternal(GetMoveCanceledFeedbackMessage(), true);
            yield break;
        }

        if (!IsCollectorWithinDistance(collector, _manualCollectMaxDistance))
        {
            CancelManualCollectInternal(GetDistanceCanceledFeedbackMessage(), true);
            yield break;
        }

        if (!CanInventoryAcceptReward(collector))
        {
            ReleaseManualCollectState(collector);
            ShowInventoryFullFeedback();
            yield break;
        }

        bool received = TryGiveReward(collector);

        ReleaseManualCollectState(collector);

        if (!received)
        {
            if (_logEnabled)
            {
                Debug.LogWarning($"[CCollectableInteractObject2D] 수동 채집 실패. itemId={_itemId}, object={name}");
            }

            RaiseCollectFailedFeedback();
            yield break;
        }

        if (_gatherSkillExp > 0)
        {
            collector.TryAddLifeSkillExp(ELifeSkill.Gathering, _gatherSkillExp);
        }

        CompleteCollect();
    }

    private void PublishManualCollectAction(ItemSO rewardItemSo, float interactionDuration)
    {
        if (rewardItemSo == null)
        {
            return;
        }

        float safeDuration = Mathf.Max(0f, interactionDuration);

        switch (rewardItemSo.Type)
        {
            case EType.WoodItem:
                OnPlayerLogging.Publish(transform.position, safeDuration);
                break;

            case EType.SeedItem:
            case EType.FeedItem:
                OnPlayerShovel.Publish(transform.position, safeDuration);
                break;

            case EType.OreItem:
                OnPlayerMining.Publish(transform.position, safeDuration);
                break;
        }
    }

    private void ReleaseManualCollectState(CPlayerCollector2D collector)
    {
        _isProcessing = false;

        if (_setPlayerBusyDuringManualCollect && collector != null)
        {
            collector.SetInteractionBusy(false);
        }

        UnsubscribeMoveCancel();

        _manualCollectRoutine = null;
        _manualCollectCollector = null;
        _manualCollectStartPosition = Vector2.zero;

        UpdateIndicatorVisibility();
    }

    private void CancelManualCollectInternal(string message, bool showFeedback)
    {
        if (_manualCollectRoutine == null)
        {
            return;
        }

        CPlayerCollector2D collector = _manualCollectCollector;

        if (_manualCollectRoutine != null)
        {
            StopCoroutine(_manualCollectRoutine);
            _manualCollectRoutine = null;
        }

        if (_logEnabled)
        {
            Debug.Log($"[CCollectableInteractObject2D] 수동 채집 취소. object={name}, message={message}");
        }

        if (_setPlayerBusyDuringManualCollect && collector != null)
        {
            collector.SetInteractionBusy(false);
        }

        _isProcessing = false;
        _manualCollectCollector = null;
        _manualCollectStartPosition = Vector2.zero;

        UnsubscribeMoveCancel();

        if (_logEnabled)
        {
            Debug.Log("[CCollectableInteractObject2D] OnPlayerCanceled.Publish");
        }

        OnPlayerCanceled.Publish();

        if (showFeedback)
        {
            RaiseCollectCanceledFeedback(message);
        }

        UpdateIndicatorVisibility();
    }


    private void RaiseCollectStartedFeedback()
    {
        TryResolveFeedbackRelayIfMissing();

        string progressMessage = GetProgressFeedbackMessage();

        if (_feedbackRelay != null)
        {
            if (IsMiningTarget())
            {
                if (!string.IsNullOrWhiteSpace(progressMessage))
                {
                    _feedbackRelay.ShowWarningFeedback(progressMessage);
                    return;
                }
            }
            else
            {
                _feedbackRelay.OnCollectStarted();
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(progressMessage))
        {
            if (_onFeedbackMessage != null)
            {
                _onFeedbackMessage.Invoke(progressMessage);
            }
            else
            {
                PublishDirectFeedback(progressMessage, EFeedbackMessageType.Warning, START_FEEDBACK_DURATION);
            }
        }

        _onCollectStarted?.Invoke();
    }

    private void RaiseCollectedSuccessFeedback()
    {
        TryResolveFeedbackRelayIfMissing();

        if (_feedbackRelay != null)
        {
            _feedbackRelay.OnCollected();
            _pendingLocalCollectedSuccessMessage = string.Empty;
            return;
        }

        if (!string.IsNullOrWhiteSpace(_pendingLocalCollectedSuccessMessage))
        {
            NotifySuccessFeedback(_pendingLocalCollectedSuccessMessage);
            _pendingLocalCollectedSuccessMessage = string.Empty;
            return;
        }

        _onCollected?.Invoke();
    }

    private void RaiseCollectCanceledFeedback(string message)
    {
        string resolvedMessage = ResolveCollectCanceledFeedbackMessage(message);

        TryResolveFeedbackRelayIfMissing();

        if (_feedbackRelay != null)
        {
            _feedbackRelay.ShowWarningFeedback(resolvedMessage);
            return;
        }

        NotifyWarningFeedback(resolvedMessage);
        _onCollectCanceled?.Invoke();
    }

    private string GetProgressFeedbackMessage()
    {
        if (IsMiningTarget())
        {
            return "채광 중...";
        }

        return "채집 중...";
    }

    private string GetMoveCanceledFeedbackMessage()
    {
        if (IsMiningTarget())
        {
            return "이동하여 채광이 취소되었습니다.";
        }

        if (!string.IsNullOrWhiteSpace(_manualCollectCancelMessage))
        {
            return _manualCollectCancelMessage;
        }

        return "이동하여 채집이 취소되었습니다.";
    }

    private string GetDistanceCanceledFeedbackMessage()
    {
        if (IsMiningTarget())
        {
            return "채광 범위를 벗어나 취소되었습니다.";
        }

        return "채집 범위를 벗어나 취소되었습니다.";
    }

    private string GetDefaultCanceledFeedbackMessage()
    {
        return IsMiningTarget()
            ? "채광을 취소했습니다."
            : "채집을 취소했습니다.";
    }

    private string ResolveCollectCanceledFeedbackMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return GetDefaultCanceledFeedbackMessage();
        }

        return message;
    }

    private void OnPlayerMoveEvent(OnPlayerMove eventData)
    {
        if (_manualCollectRoutine == null)
        {
            return;
        }

        if (eventData.moved.sqrMagnitude <= _moveCancelInputThreshold)
        {
            return;
        }

        CancelManualCollectInternal(GetMoveCanceledFeedbackMessage(), true);
    }

    private void SubscribeMoveCancel()
    {
        if (_isMoveCancelSubscribed)
        {
            return;
        }

        EventBus<OnPlayerMove>.Subscribe(OnPlayerMoveEvent);
        _isMoveCancelSubscribed = true;
    }

    private void UnsubscribeMoveCancel()
    {
        if (!_isMoveCancelSubscribed)
        {
            return;
        }

        EventBus<OnPlayerMove>.Unsubscribe(OnPlayerMoveEvent);
        _isMoveCancelSubscribed = false;
    }

    private bool HasCollectorMovedSinceStart(CPlayerCollector2D collector)
    {
        if (collector == null)
        {
            return false;
        }

        Vector2 currentPos = collector.transform.position;
        float movedSqrDistance = (currentPos - _manualCollectStartPosition).sqrMagnitude;
        return movedSqrDistance > _moveCancelPositionThreshold;
    }

    private ItemSO ResolveRewardItemSO()
    {
        if (DatabaseManager.Ins == null || string.IsNullOrWhiteSpace(_itemId))
        {
            return null;
        }

        return DatabaseManager.Ins.Item(_itemId);
    }

    private void PublishRecordEventByReward()
    {
        if (string.IsNullOrWhiteSpace(_itemId))
        {
            return;
        }

        if (IsMiningTarget())
        {
            OnPlayerMined.Publish(_itemId);
        }
        else
        {
            OnPlayerCollected.Publish(_itemId);
        }
    }

    private bool IsMiningTarget()
    {
        ItemSO rewardItemSo = ResolveRewardItemSO();
        return rewardItemSo != null && rewardItemSo.Type == EType.OreItem;
    }

    private string GetTargetDisplayName()
    {
        return string.IsNullOrWhiteSpace(_displayName) ? _itemId : _displayName;
    }

    private string GetActionDisplayName()
    {
        return IsMiningTarget() ? "채광" : "채집";
    }

    private EType ResolveRequiredToolType()
    {
        if (!_requireEquippedTool)
        {
            return EType.None;
        }

        if (_requiredToolType != EType.None)
        {
            return _requiredToolType;
        }

        ItemSO rewardItemSO = ResolveRewardItemSO();
        if (rewardItemSO == null)
        {
            return EType.None;
        }

        switch (rewardItemSO.Type)
        {
            case EType.OreItem:
                return EType.PickaxeItem;

            case EType.WoodItem:
                return EType.AxeItem;

            case EType.FeedItem:
                return EType.ShovelItem;

            case EType.HarvestItem:
            case EType.ProductItem:
            case EType.AnimalItem:
                return EType.SickleItem;

            default:
                return EType.None;
        }
    }

    private ERarity ResolveRequiredToolRarity()
    {
        if (_useRewardItemRarityAsRequiredToolRarity)
        {
            ItemSO rewardItemSO = ResolveRewardItemSO();
            if (rewardItemSO != null && rewardItemSO.Rarity != ERarity.None)
            {
                return rewardItemSO.Rarity;
            }
        }

        return _requiredToolMinRarity == ERarity.None ? ERarity.Basic : _requiredToolMinRarity;
    }

    private float GetManualCollectDuration()
    {
        float baseDuration = Mathf.Max(0f, _manualCollectDelay);

        if (!_useToolRarityBasedDuration)
        {
            return baseDuration;
        }

        EType requiredToolType = ResolveRequiredToolType();
        if (requiredToolType == EType.None)
        {
            return baseDuration;
        }

        if (!TryGetBestToolInInventory(requiredToolType, out ToolItemSO bestTool) || bestTool == null)
        {
            return baseDuration;
        }

        ERarity requiredRarity = ResolveRequiredToolRarity();
        return CalculateManualCollectDuration(requiredRarity, bestTool.Rarity);
    }

    private float CalculateManualCollectDuration(ERarity targetRarity, ERarity toolRarity)
    {
        float baseDuration = Mathf.Max(0f, _manualCollectDelay);
        float minDuration = Mathf.Clamp(_manualCollectMinDuration, 0f, baseDuration);
        float reducePerStep = Mathf.Max(0f, _manualCollectDurationReducePerRarityStep);

        int rarityGap = Mathf.Max(0, GetComparableRarityValue(toolRarity) - GetComparableRarityValue(targetRarity));
        float calculatedDuration = baseDuration - reducePerStep * rarityGap;

        return Mathf.Max(minDuration, calculatedDuration);
    }

    private int GetComparableRarityValue(ERarity rarity)
    {
        switch (rarity)
        {
            case ERarity.Basic:
                return 1;

            case ERarity.Solid:
                return 2;

            case ERarity.Superior:
                return 3;

            case ERarity.Prime:
                return 4;

            case ERarity.Masterwork:
                return 5;

            default:
                return 0;
        }
    }

    private bool TryValidateRequiredTool(
        out EType requiredToolType,
        out bool isRarityLow,
        out string failMessage)
    {
        requiredToolType = ResolveRequiredToolType();
        isRarityLow = false;
        failMessage = string.Empty;

        if (requiredToolType == EType.None)
        {
            return true;
        }

        if (!TryGetBestToolInInventory(requiredToolType, out ToolItemSO bestTool))
        {
            failMessage = GetMissingToolMessage(requiredToolType);
            return false;
        }

        ERarity requiredRarity = ResolveRequiredToolRarity();
        if (bestTool.Rarity < requiredRarity)
        {
            isRarityLow = true;
            failMessage = GetLowRarityMessage(requiredToolType);
            return false;
        }

        return true;
    }

    private bool TryGetBestToolInInventory(EType toolType, out ToolItemSO bestTool)
    {
        bestTool = null;

        if (InventoryManager.Ins == null || InventoryManager.Ins.PlayerInventory == null)
        {
            return false;
        }

        Inventory inventory = InventoryManager.Ins.PlayerInventory;
        InventorySlot[] slots = inventory.InventorySlots;

        if (slots == null || slots.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];

            if (slot.IsEmpty)
            {
                continue;
            }

            ToolItemSO candidate = slot.ItemSO as ToolItemSO;
            if (candidate == null)
            {
                continue;
            }

            if (candidate.Type != toolType)
            {
                continue;
            }

            if (bestTool == null || IsBetterTool(candidate, bestTool))
            {
                bestTool = candidate;
            }
        }

        return bestTool != null;
    }

    private bool IsBetterTool(ToolItemSO candidate, ToolItemSO currentBest)
    {
        if (candidate == null)
        {
            return false;
        }

        if (currentBest == null)
        {
            return true;
        }

        if (candidate.Rarity != currentBest.Rarity)
        {
            return candidate.Rarity > currentBest.Rarity;
        }

        if (candidate.CurrentLv != currentBest.CurrentLv)
        {
            return candidate.CurrentLv > currentBest.CurrentLv;
        }

        if (!Mathf.Approximately(candidate.Strength, currentBest.Strength))
        {
            return candidate.Strength > currentBest.Strength;
        }

        return string.Compare(candidate.Id, currentBest.Id, System.StringComparison.Ordinal) < 0;
    }

    private string GetMissingToolMessage(EType requiredToolType)
    {
        if (IsMiningTarget() && requiredToolType == EType.PickaxeItem)
        {
            return "곡괭이가 없습니다.";
        }

        switch (requiredToolType)
        {
            case EType.PickaxeItem:
                return "곡괭이가 필요합니다.";

            case EType.ShovelItem:
                return "삽이 없습니다.";

            case EType.SickleItem:
                return "낫이 필요합니다.";

            case EType.AxeItem:
                return "도끼가 필요합니다.";

            default:
                return "도구가 필요합니다.";
        }
    }

    private string GetLowRarityMessage(EType requiredToolType)
    {
        if (IsMiningTarget() && requiredToolType == EType.PickaxeItem)
        {
            return "도구 등급이 낮습니다.";
        }

        return "도구 등급이 낮습니다.";
    }

    private string GetRequiredToolGuideMessage()
    {
        EType requiredToolType = ResolveRequiredToolType();
        if (requiredToolType == EType.None)
        {
            return string.Empty;
        }

        ERarity requiredRarity = ResolveRequiredToolRarity();
        return $"{GetRarityDisplayName(requiredRarity)} 등급 이상의 {GetToolTypeDisplayName(requiredToolType)} 필요";
    }

    private string GetToolTypeDisplayName(EType toolType)
    {
        switch (toolType)
        {
            case EType.SickleItem:
                return "낫";

            case EType.ShovelItem:
                return "삽";

            case EType.AxeItem:
                return "도끼";

            case EType.PickaxeItem:
                return "곡괭이";

            case EType.WateringCan:
                return "물뿌리개";

            case EType.Fishingrod:
                return "낚싯대";

            default:
                return "도구";
        }
    }

    private string GetRarityDisplayName(ERarity rarity)
    {
        switch (rarity)
        {
            case ERarity.Basic:
                return "기본";

            case ERarity.Solid:
                return "견고";

            case ERarity.Superior:
                return "고급";

            case ERarity.Prime:
                return "프라임";

            case ERarity.Masterwork:
                return "마스터워크";

            default:
                return "기본";
        }
    }

    private bool IsCollectorWithinDistance(CPlayerCollector2D collector, float maxDistance)
    {
        if (collector == null)
        {
            return false;
        }

        if (_triggerCollider == null)
        {
            return false;
        }

        if (maxDistance <= 0f)
        {
            return false;
        }

        Vector2 collectorPos = collector.transform.position;
        Vector2 closestPoint = _triggerCollider.ClosestPoint(collectorPos);
        float sqrDistance = (collectorPos - closestPoint).sqrMagnitude;

        return sqrDistance <= maxDistance * maxDistance;
    }

    private bool CanInventoryAcceptReward(CPlayerCollector2D collector)
    {
        if (collector == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_itemId))
        {
            return false;
        }

        if (_amount <= 0)
        {
            return false;
        }

        if (InventoryManager.Ins == null || InventoryManager.Ins.PlayerInventory == null)
        {
            return false;
        }

        ItemSO rewardItemSO = ResolveRewardItemSO();
        if (rewardItemSO == null)
        {
            return false;
        }

        Inventory inventory = InventoryManager.Ins.PlayerInventory;
        InventorySlot[] slots = inventory.InventorySlots;

        if (slots == null || slots.Length == 0)
        {
            return false;
        }

        int remainAmount = _amount;
        int maxStack = Mathf.Max(1, rewardItemSO.MaxStack);

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];

            if (slot.IsEmpty)
            {
                continue;
            }

            if (slot.ItemSO == null)
            {
                continue;
            }

            if (slot.ItemSO.Id != rewardItemSO.Id)
            {
                continue;
            }

            int remainStack = maxStack - slot.CurStack;
            if (remainStack <= 0)
            {
                continue;
            }

            remainAmount -= remainStack;

            if (remainAmount <= 0)
            {
                return true;
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];

            if (!slot.IsEmpty)
            {
                continue;
            }

            remainAmount -= maxStack;

            if (remainAmount <= 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGiveReward(CPlayerCollector2D collector)
    {
        if (collector == null)
        {
            Debug.LogWarning($"[CCollectableInteractObject2D] collector가 null입니다. object={name}");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_itemId))
        {
            Debug.LogWarning($"[CCollectableInteractObject2D] itemId가 비어 있습니다. object={name}");
            return false;
        }

        if (_amount <= 0)
        {
            Debug.LogWarning($"[CCollectableInteractObject2D] amount가 0 이하입니다. object={name}");
            return false;
        }

        bool received = collector.TryReceiveItem(_itemId, _amount);

        if (!received)
        {
            if (_logEnabled)
            {
                Debug.LogWarning($"[CCollectableInteractObject2D] 아이템 지급 실패. itemId={_itemId}, amount={_amount}, object={name}");
            }

            return false;
        }

        PublishRecordEventByReward();
        return true;
    }

    private void StartAutoCollect()
    {
        if (_collectMode == ECollectMode.ButtonOnly)
        {
            return;
        }

        if (!CanAutoCollect)
        {
            return;
        }

        if (_autoCollectRoutine != null)
        {
            return;
        }

        if (_isCollected || _isProcessing)
        {
            return;
        }

        _autoCollectRoutine = StartCoroutine(CoAutoCollect());
    }

    private void StopAutoCollect()
    {
        if (_autoCollectRoutine == null)
        {
            return;
        }

        StopCoroutine(_autoCollectRoutine);
        _autoCollectRoutine = null;
    }

    private IEnumerator CoAutoCollect()
    {
        if (_autoCollectDelay > 0f)
        {
            yield return UCoroutine.GetWait(_autoCollectDelay);
        }

        _autoCollectRoutine = null;

        if (_collectMode == ECollectMode.ButtonOnly)
        {
            yield break;
        }

        if (_isCollected || _isProcessing)
        {
            yield break;
        }

        for (int i = 0; i < _nearCollectors.Count; i++)
        {
            CPlayerCollector2D collector = _nearCollectors[i];

            if (collector == null)
            {
                continue;
            }

            if (!IsCollectorWithinDistance(collector, _autoCollectMaxDistance))
            {
                continue;
            }

            if (TryCollectInternal(collector, false))
            {
                yield break;
            }
        }

        UpdateIndicatorVisibility();
    }

    private void CompleteCollect()
    {
        if (_isCollected)
        {
            return;
        }

        _isCollected = true;

        _nearCollectors.Clear();
        StopAutoCollect();
        ApplyIndicatorActive(false);

        if (_triggerCollider != null)
        {
            _triggerCollider.enabled = false;
        }

        if (_logEnabled)
        {
            Debug.Log($"[CCollectableInteractObject2D] 획득 완료. itemId={_itemId}, amount={_amount}, object={name}");
        }

        QueueCollectedSuccessFeedbackMessage();
        RaiseCollectedSuccessFeedback();
        InteractionFeedbackRelay.ClearQueuedCollectSuccessMessages();

        if (_useRespawn)
        {
            SetCollectedVisual(false);

            if (_respawnRoutine != null)
            {
                StopCoroutine(_respawnRoutine);
            }

            _respawnRoutine = StartCoroutine(CoRespawn());
            return;
        }

        if (_destroyOnCollected)
        {
            UObject.Destroy(gameObject);
            return;
        }

        if (_disableTargetAfterCollected != null)
        {
            _disableTargetAfterCollected.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator CoRespawn()
    {
        if (_respawnDelay > 0f)
        {
            yield return new WaitForSeconds(_respawnDelay);
        }

        _respawnRoutine = null;
        ResetCollectState();
    }

    private void ResetCollectState()
    {
        _isCollected = false;
        _isProcessing = false;

        if (_triggerCollider != null)
        {
            _triggerCollider.enabled = true;
        }

        SetCollectedVisual(true);
        RebuildCollectorListInRange();

        if (_logEnabled)
        {
            Debug.Log($"[CCollectableInteractObject2D] 리스폰 완료. itemId={_itemId}, object={name}");
        }

        _onRespawned?.Invoke();

        if (CanAutoCollect && HasAnyCollectorInRange())
        {
            StartAutoCollect();
        }

        UpdateIndicatorVisibility();
    }

    private void SetCollectedVisual(bool isVisible)
    {
        if (_disableTargetAfterCollected != null)
        {
            _disableTargetAfterCollected.SetActive(isVisible);
            return;
        }

        if (_useRespawn && _logEnabled)
        {
            Debug.LogWarning($"[CCollectableInteractObject2D] 리스폰을 쓰는데 _disableTargetAfterCollected가 비어 있습니다. object={name}");
        }
    }

    private void CleanupCollectors()
    {
        for (int i = _nearCollectors.Count - 1; i >= 0; --i)
        {
            if (_nearCollectors[i] == null)
            {
                _nearCollectors.RemoveAt(i);
            }
        }
    }

    private bool HasAnyCollectorInRange()
    {
        CleanupCollectors();
        return _nearCollectors.Count > 0;
    }

    private void RebuildCollectorListInRange()
    {
        _nearCollectors.Clear();

        if (_triggerCollider == null)
        {
            return;
        }

        Bounds bounds = _triggerCollider.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);

        if (hits == null || hits.Length == 0)
        {
            return;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
            {
                continue;
            }

            CPlayerCollector2D collector = hit.GetComponentInParent<CPlayerCollector2D>();

            if (collector == null)
            {
                continue;
            }

            if (_nearCollectors.Contains(collector))
            {
                continue;
            }

            _nearCollectors.Add(collector);
        }
    }

    /// <summary>
    /// 지금은 로그 + 이벤트만 호출
    /// 나중에 UI 스크립트가 생기면 Inspector에서 바로 연결 가능
    /// </summary>
    private void QueueCollectedSuccessFeedbackMessage()
    {
        string itemName = ResolveCollectedFeedbackItemName();
        _pendingLocalCollectedSuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(itemName))
        {
            return;
        }

        TryResolveFeedbackRelayIfMissing();

        if (IsMiningTarget())
        {
            if (_feedbackRelay != null)
            {
                InteractionFeedbackRelay.QueueMiningSuccessMessage(itemName);
                return;
            }

            _pendingLocalCollectedSuccessMessage = BuildCollectedSuccessFeedbackMessage("채광", itemName);
            return;
        }

        if (_feedbackRelay != null)
        {
            InteractionFeedbackRelay.QueueCollectSuccessMessage(itemName);
            return;
        }

        _pendingLocalCollectedSuccessMessage = BuildCollectedSuccessFeedbackMessage("채집", itemName);
    }

    private string BuildCollectedSuccessFeedbackMessage(string prefix, string itemName)
    {
        string safePrefix = string.IsNullOrWhiteSpace(prefix) ? "획득" : prefix.Trim();
        string safeItemName = string.IsNullOrWhiteSpace(itemName) ? "아이템" : itemName.Trim();
        return $"{safePrefix}: {safeItemName} 획득";
    }

    private string ResolveCollectedFeedbackItemName()
    {
        if (!string.IsNullOrWhiteSpace(_itemId) && DatabaseManager.Ins != null)
        {
            ItemSO itemSo = DatabaseManager.Ins.Item(_itemId);
            if (itemSo != null && !string.IsNullOrWhiteSpace(itemSo.Name))
            {
                return itemSo.Name.Trim();
            }
        }

        if (!string.IsNullOrWhiteSpace(_itemId))
        {
            return _itemId.Trim();
        }

        return IsMiningTarget() ? "광석" : "채집물";
    }

    private void ShowReservedFeedback(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (_logEnabled)
        {
            Debug.Log("[Collectable][Feedback] " + message);
        }
        OnPlayerCanceled.Publish();
        _onFeedbackMessage?.Invoke(message);
    }

    /// <summary>
    /// 현재 표시를 켜야 하는지 판단
    /// - 수동 채집 가능한 모드일 것
    /// - 아직 채집되지 않았을 것
    /// - 채집 처리 중이 아닐 것
    /// - 범위 안에 실제로 채집 가능한 플레이어가 있을 것
    /// </summary>

    private bool ShouldShowCollectIndicator()
    {
        if (_canCollectIndicatorObject == null)
        {
            return false;
        }

        if (_isCollected)
        {
            return false;
        }

        if (!CanManualCollect)
        {
            return false;
        }

        if (_hideIndicatorWhileCollecting && (_manualCollectRoutine != null || _isProcessing))
        {
            return false;
        }

        CleanupCollectors();
        return _nearCollectors.Count > 0;
    }

    private void UpdateIndicatorVisibility()
    {
        if (_canCollectIndicatorObject == null)
        {
            return;
        }

        ApplyIndicatorActive(ShouldShowCollectIndicator());
    }

    private void ApplyIndicatorActive(bool isActive)
    {
        if (_canCollectIndicatorObject == null)
        {
            return;
        }

        if (_canCollectIndicatorObject.activeSelf == isActive)
        {
            return;
        }

        _canCollectIndicatorObject.SetActive(isActive);
    }

    private void RaiseToolConditionFailFeedback(EType requiredToolType, bool isRarityLow, string fallbackMessage)
    {
        TryResolveFeedbackRelayIfMissing();

        string relayMethodName = ResolveToolConditionFailRelayMethodName(requiredToolType, isRarityLow);

        if (HasRelayListener(_onCollectFailed, relayMethodName))
        {
            _onCollectFailed?.Invoke();
            return;
        }

        if (_feedbackRelay != null)
        {
            if (IsMiningTarget())
            {
                if (isRarityLow)
                {
                    _feedbackRelay.OnMiningToolRarityLow();
                }
                else if (requiredToolType == EType.PickaxeItem)
                {
                    _feedbackRelay.OnMiningNoPickaxe();
                }
                else
                {
                    _feedbackRelay.OnMiningFailed();
                }
            }
            else
            {
                if (isRarityLow)
                {
                    _feedbackRelay.OnCollectToolRarityLow();
                }
                else
                {
                    switch (requiredToolType)
                    {
                        case EType.PickaxeItem:
                            _feedbackRelay.OnCollectNeedPickaxe();
                            break;

                        case EType.ShovelItem:
                            _feedbackRelay.OnCollectNeedShovel();
                            break;

                        case EType.SickleItem:
                            _feedbackRelay.OnCollectNeedSickle();
                            break;

                        case EType.AxeItem:
                            _feedbackRelay.OnCollectNeedAxe();
                            break;

                        default:
                            _feedbackRelay.OnCollectFailed();
                            break;
                    }
                }
            }
        }
        else
        {
            NotifyWarningFeedback(fallbackMessage);
        }

        _onCollectFailed?.Invoke();
    }

    private void ShowInventoryFullFeedback()
    {
        TryResolveFeedbackRelayIfMissing();

        if (_feedbackRelay != null)
        {
            _feedbackRelay.OnInventoryFull();
            return;
        }

        NotifyFailureFeedback("인벤토리가 가득 찼습니다.");
    }

    private void RaiseCollectFailedFeedback()
    {
        TryResolveFeedbackRelayIfMissing();

        string relayMethodName = IsMiningTarget()
            ? nameof(InteractionFeedbackRelay.OnMiningFailed)
            : nameof(InteractionFeedbackRelay.OnCollectFailed);

        if (HasRelayListener(_onCollectFailed, relayMethodName))
        {
            _onCollectFailed?.Invoke();
            return;
        }

        if (_feedbackRelay != null)
        {
            if (IsMiningTarget())
            {
                _feedbackRelay.OnMiningFailed();
            }
            else
            {
                _feedbackRelay.OnCollectFailed();
            }
        }
        else
        {
            if (IsMiningTarget())
            {
                NotifyFailureFeedback("채광에 실패하였습니다.");
            }
            else
            {
                NotifyFailureFeedback("채집에 실패하였습니다.");
            }
        }

        _onCollectFailed?.Invoke();
    }

    private string ResolveToolConditionFailRelayMethodName(EType requiredToolType, bool isRarityLow)
    {
        if (IsMiningTarget())
        {
            if (isRarityLow)
            {
                return nameof(InteractionFeedbackRelay.OnMiningToolRarityLow);
            }

            if (requiredToolType == EType.PickaxeItem)
            {
                return nameof(InteractionFeedbackRelay.OnMiningNoPickaxe);
            }

            return nameof(InteractionFeedbackRelay.OnMiningFailed);
        }

        if (isRarityLow)
        {
            return nameof(InteractionFeedbackRelay.OnCollectToolRarityLow);
        }

        switch (requiredToolType)
        {
            case EType.PickaxeItem:
                return nameof(InteractionFeedbackRelay.OnCollectNeedPickaxe);

            case EType.ShovelItem:
                return nameof(InteractionFeedbackRelay.OnCollectNeedShovel);

            case EType.SickleItem:
                return nameof(InteractionFeedbackRelay.OnCollectNeedSickle);

            case EType.AxeItem:
                return nameof(InteractionFeedbackRelay.OnCollectNeedAxe);

            default:
                return nameof(InteractionFeedbackRelay.OnCollectFailed);
        }
    }

    private bool HasRelayListener(UnityEvent unityEvent, params string[] methodNames)
    {
        if (unityEvent == null || _feedbackRelay == null)
        {
            return false;
        }

        int persistentCount = unityEvent.GetPersistentEventCount();
        if (persistentCount <= 0)
        {
            return false;
        }

        bool hasMethodFilter = methodNames != null && methodNames.Length > 0;

        for (int i = 0; i < persistentCount; i++)
        {
            Object target = unityEvent.GetPersistentTarget(i);
            if (target != _feedbackRelay)
            {
                continue;
            }

            if (!hasMethodFilter)
            {
                return true;
            }

            string methodName = unityEvent.GetPersistentMethodName(i);
            for (int j = 0; j < methodNames.Length; j++)
            {
                string candidate = methodNames[j];
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (string.Equals(methodName, candidate, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void NotifyWarningFeedback(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (_logEnabled)
        {
            Debug.Log("[Collectable][Warning] " + message);
        }

        TryResolveFeedbackRelayIfMissing();

        if (_feedbackRelay != null)
        {
            _feedbackRelay.ShowWarningFeedback(message);
            return;
        }

        if (_onFeedbackMessage != null)
        {
            _onFeedbackMessage.Invoke(message);
        }
        else
        {
            PublishDirectFeedback(message, EFeedbackMessageType.Warning, DEFAULT_FEEDBACK_DURATION);
        }

        PlayRandomFeedbackSound(_warningFeedbackSoundIds);
    }

    private void NotifyFailureFeedback(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (_logEnabled)
        {
            Debug.Log("[Collectable][Failure] " + message);
        }

        TryResolveFeedbackRelayIfMissing();

        if (_feedbackRelay != null)
        {
            _feedbackRelay.ShowFailureFeedback(message);
            return;
        }

        if (_onFeedbackMessage != null)
        {
            _onFeedbackMessage.Invoke(message);
        }
        else
        {
            PublishDirectFeedback(message, EFeedbackMessageType.Failure, DEFAULT_FEEDBACK_DURATION);
        }

        PlayRandomFeedbackSound(_failureFeedbackSoundIds);
    }

    private void NotifySuccessFeedback(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (_logEnabled)
        {
            Debug.Log("[Collectable][Success] " + message);
        }

        TryResolveFeedbackRelayIfMissing();

        if (_feedbackRelay != null)
        {
            _feedbackRelay.ShowSuccessFeedback(message);
            return;
        }

        if (_onFeedbackMessage != null)
        {
            _onFeedbackMessage.Invoke(message);
        }
        else
        {
            PublishDirectFeedback(message, EFeedbackMessageType.Success, SUCCESS_FEEDBACK_DURATION);
        }

        PlayRandomFeedbackSound(_successFeedbackSoundIds);
    }

    private void PlayRandomFeedbackSound(string[] soundIds)
    {
        if (soundIds == null || soundIds.Length <= 0)
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
            string soundId = soundIds[i];
            if (string.IsNullOrWhiteSpace(soundId))
            {
                continue;
            }

            if (currentIndex == targetIndex)
            {
                if (_feedbackSoundTarget != null)
                {
                    USound.PlaySfx(soundId, _feedbackSoundTarget);
                }
                else
                {
                    USound.PlaySfx(soundId);
                }

                return;
            }

            currentIndex++;
        }
    }

    private void TryResolveFeedbackRelayIfMissing()
    {
        if (_feedbackRelay != null)
        {
            return;
        }

        _feedbackRelay = FindObjectOfType<InteractionFeedbackRelay>();

        if (_feedbackRelay != null && _logEnabled)
        {
            Debug.Log($"[CCollectableInteractObject2D] InteractionFeedbackRelay 자동 연결. object={name}, relay={_feedbackRelay.name}");
        }
    }

    private void PublishDirectFeedback(string message, EFeedbackMessageType messageType, float duration)
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

        if (safeMessage.Length > 30)
        {
            safeMessage = safeMessage.Substring(0, 30);
        }

        OnFeedbackMessageRequested.Publish(safeMessage, messageType, Mathf.Max(0f, duration));
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        _triggerCollider = GetComponent<Collider2D>();
        TryResolveFeedbackRelayIfMissing();

        if (_feedbackSoundTarget == null)
        {
            _feedbackSoundTarget = transform;
        }

        if (_triggerCollider != null && !_triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[CCollectableInteractObject2D] {name}의 Collider2D가 Trigger가 아닙니다. 자동/버튼 채집 범위 감지가 안 될 수 있습니다.");
        }

        ApplyIndicatorActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isCollected)
        {
            return;
        }

        CPlayerCollector2D collector = other.GetComponentInParent<CPlayerCollector2D>();

        if (collector == null)
        {
            return;
        }

        if (!_nearCollectors.Contains(collector))
        {
            _nearCollectors.Add(collector);
        }

        UpdateIndicatorVisibility();

        if (CanAutoCollect)
        {
            StartAutoCollect();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_isCollected)
        {
            return;
        }

        CPlayerCollector2D collector = other.GetComponentInParent<CPlayerCollector2D>();

        if (collector == null)
        {
            return;
        }

        if (!_nearCollectors.Contains(collector))
        {
            _nearCollectors.Add(collector);
        }

        UpdateIndicatorVisibility();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        CPlayerCollector2D collector = other.GetComponentInParent<CPlayerCollector2D>();

        if (collector == null)
        {
            return;
        }

        _nearCollectors.Remove(collector);
        UpdateIndicatorVisibility();

        if (!HasAnyCollectorInRange())
        {
            StopAutoCollect();
        }
    }

    private void OnDisable()
    {
        CancelManualCollectInternal(string.Empty, false);

        StopAutoCollect();
        ApplyIndicatorActive(false);

        if (_respawnRoutine != null)
        {
            StopCoroutine(_respawnRoutine);
            _respawnRoutine = null;
        }
    }
    #endregion
}
