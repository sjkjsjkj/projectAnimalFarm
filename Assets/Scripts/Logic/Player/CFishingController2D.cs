using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 낚시 컨트롤러
///
/// 수정 포인트
/// 1. 낚시 실패 사유를 InteractionFeedbackRelay로 중앙 관리
/// 2. 낚싯대 / 미끼 / 등급 / 인벤토리 상태에 따라 상황별 메시지 출력
/// 3. 손에 든 아이템이 아니라 인벤토리 내 대표 낚싯대 기준으로 판정
/// 4. FishingSpot의 CanInteract 단계에서 막히지 않도록 수동 가능 여부는 느슨하게 허용
/// 5. 바다 낚시 시 미끼의 바다 사용 가능 여부도 검사
/// 6. 낚시 성공 시 피드백 UI에 획득한 물고기 이름이 표시되도록 추가
/// 7. 기본 성공 메시지가 이름 피드백을 덮지 않도록 이름 피드백을 가장 마지막에 전송
/// </summary>
public class CFishingController2D : BaseMono
{
    [Serializable]
    public class StringEvent : UnityEvent<string> { }

    private enum EFishingAreaType
    {
        None = 0,
        FreshWater,
        SeaWater
    }

    private enum EFishingStartResult
    {
        Success = 0,
        AlreadyFishing,
        CollectorNull,
        CollectorBusy,
        Cooldown,
        NotEnoughHunger,
        DatabaseMissing,
        InventoryMissing,
        NoFishingArea,
        NoFishingRodInQuickSlot,
        NoFishingRodInInventory,
        NoBaitInInventory,
        BaitInsufficient,
        NoCandidateFish,
        NoCatchableFishForQuickSlotRod,
        NoCatchableFishForBait,
        RewardItemDataMissing,
        InventoryFull
    }

    [Header("낚시 대상 DB")]
    [SerializeField] private SheetItemDatabase _database;

    [Header("기존 타일 기반 낚시 판정")]
    [SerializeField] private float _checkDistance = 0.9f;
    [SerializeField] private Vector2 _checkOffset = Vector2.zero;
    [SerializeField] private bool _allowSeaFishing = true;

    [Header("낚시 연출")]
    [SerializeField] private float _fishingDelay = 1.5f;
    [SerializeField] private float _cooldownAfterFishing = 0.2f;

    [Header("지급 설정")]
    [SerializeField] private int _amount = 1;
    [SerializeField] private int _fishingSkillExp = 0;

    [Header("배고픔 요구 조건")]
    [Tooltip("체크하면 낚시 시작 시 배고픔을 검사하고 소모합니다.")]
    [SerializeField] private bool _useHungerCost = true;

    [Tooltip("1회 낚시에 필요한 배고픔 수치")]
    [SerializeField, Min(0f)] private float _requiredHunger = 10f;

    [Tooltip("배고픔이 부족할 때 출력할 문구")]
    [SerializeField] private string _notEnoughHungerMessage = "배고픔이 부족해 낚시할 수 없습니다.";

    [Header("낚시 도구 / 미끼 요구 조건")]
    [Tooltip("체크하면 인벤토리의 대표 낚싯대가 있어야 낚시 가능")]
    [SerializeField] private bool _requireFishingRodInQuickSlot = true;

    [Tooltip("체크하면 대표 낚싯대 등급 이하의 물고기만 낚임")]
    [SerializeField] private bool _limitCatchByQuickSlotRodRarity = true;

    [Tooltip("체크하면 미끼가 있어야 낚시 가능")]
    [SerializeField] private bool _requireBait = true;

    [Tooltip("1회 낚시당 필요한 미끼 개수")]
    [SerializeField] private int _requiredBaitCount = 1;

    [Tooltip("체크하면 낚시 성공 시 미끼를 소모")]
    [SerializeField] private bool _consumeBaitOnSuccess = true;

    [Tooltip("체크하면 미끼 등급 이하의 물고기만 낚임")]
    [SerializeField] private bool _limitCatchByBaitRarity = true;

    [Header("테스트용 우회 설정")]
    [Tooltip("체크하면 낚싯대 / 미끼가 인벤토리에 없어도 낚시를 시작할 수 있습니다.")]
    [SerializeField] private bool _bypassRequiredFishingItemsForTest = false;

    [Header("스폿 허용 장비 / 미끼 ID")]
    [Tooltip("비워두면 민물 스폿에서 모든 낚싯대를 허용합니다.")]
    [SerializeField] private string[] _freshWaterAllowedRodIds;

    [Tooltip("비워두면 바다 스폿에서 모든 낚싯대를 허용합니다.")]
    [SerializeField] private string[] _seaWaterAllowedRodIds;

    [Tooltip("비워두면 민물 스폿에서 모든 미끼를 허용합니다.")]
    [SerializeField] private string[] _freshWaterAllowedBaitIds;

    [Tooltip("비워두면 바다 스폿에서 모든 미끼를 허용합니다.")]
    [SerializeField] private string[] _seaWaterAllowedBaitIds;

    [Header("수동 낚시 스폿 탐색")]
    [Tooltip("수동 낚시 시 플레이어 앞쪽에서 FishingSpot을 찾는 반경")]
    [SerializeField] private float _manualSpotSearchRadius = 0.6f;

    [Header("피드백 릴레이")]
    [SerializeField] private InteractionFeedbackRelay _feedbackRelay;

    [Header("모션 이벤트 설정")]
    [Tooltip("낚시 시작 시 OnPlayerFishing.Publish(pos, duration, isSuccess)로 넘길 isSuccess 값")]
    [SerializeField] private bool _motionStartIsSuccessValue = true;

    [Tooltip("모션 이벤트로 넘길 길이값")]
    [SerializeField] private float _fishingMotionDuration = 2.5f;

    [Tooltip("플레이어가 바라보는 방향으로 모션 목표 위치를 계산할지 여부")]
    [SerializeField] private bool _useFacingDirectionMotionTarget = true;

    [Tooltip("플레이어 위치 기준으로 바라보는 방향 앞쪽에 얼마나 던질지")]
    [SerializeField] private float _motionTargetDistance = 1.1f;

    [Tooltip("최종 모션 목표 위치에 추가할 보정값")]
    [SerializeField] private Vector2 _motionTargetOffset = Vector2.zero;

    [Header("이동 취소 설정")]
    [Tooltip("이동 입력의 sqrMagnitude가 이 값보다 크면 취소")]
    [SerializeField] private float _moveCancelInputThreshold = 0.0001f;

    [Tooltip("시작 위치와 현재 위치 차이의 sqrMagnitude가 이 값보다 크면 취소")]
    [SerializeField] private float _moveCancelPositionThreshold = 0.0001f;

    [SerializeField] private string _moveCancelMessage = "이동하여 낚시가 취소되었습니다.";

    [Header("안내 문구")]
    [SerializeField] private string _freshWaterPromptText = "낚시하기";
    [SerializeField] private string _seaWaterPromptText = "바다 낚시하기";

    [Header("희귀도 가중치")]
    [SerializeField] private float _basicWeight = 70f;
    [SerializeField] private float _solidWeight = 40f;
    [SerializeField] private float _superiorWeight = 18f;
    [SerializeField] private float _primeWeight = 8f;
    [SerializeField] private float _masterworkWeight = 2f;
    [SerializeField] private float _unknownWeight = 5f;

    [Header("나중에 연결할 이벤트")]
    [Tooltip("기존 Inspector 연결 유지용 string 이벤트")]
    [SerializeField] private StringEvent _onFeedbackMessage;

    [Header("획득 피드백")]
    [SerializeField] private bool _showCatchSuccessFeedback = true;
    [SerializeField] private string _fishCatchSuccessSingleFormat = "{0} 획득";
    [SerializeField] private string _fishCatchSuccessMultipleFormat = "{0} x{1} 획득";

    [Tooltip("낚시 시작 시 호출. 나중에 사운드/VFX 연결 가능")]
    [SerializeField] private UnityEvent _onFishingStarted;

    [Tooltip("낚시 성공 시 호출. 나중에 사운드/VFX 연결 가능")]
    [SerializeField] private UnityEvent _onFishingSucceeded;

    [Tooltip("낚시 실패 시 호출. 나중에 사운드/VFX 연결 가능")]
    [SerializeField] private UnityEvent _onFishingFailed;

    [Tooltip("낚시 취소 시 호출. 나중에 사운드/VFX 연결 가능")]
    [SerializeField] private UnityEvent _onFishingCanceled;

    [Header("로그")]
    [SerializeField] private bool _logEnabled = true;

    private bool _isFishing = false;
    private float _cooldownEndTime = 0f;
    private Coroutine _fishingRoutine = null;
    private CPlayerCollector2D _activeCollector = null;
    private Vector2 _fishingStartPosition = Vector2.zero;
    private bool _isMoveCancelSubscribed = false;
    private string _lastFailMessage = string.Empty;

    private static readonly FieldInfo s_fishingSpotTypeField = typeof(CFishingSpotInteractObject2D).GetField("_fishingSpotType", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo s_baitCatchFishRarityField = typeof(BaitItemSO).GetField("_catchFishRarity", BindingFlags.NonPublic | BindingFlags.Instance);

    public string LastFailMessage => _lastFailMessage;

    public bool CanManualFish(CPlayerCollector2D collector)
    {
        return collector != null && collector.FishingController != null;
    }

    public string GetInteractionMessage(KeyCode key, CPlayerCollector2D collector)
    {
        if (collector == null)
        {
            return string.Empty;
        }

        if (!TryGetFishingAreaType(collector, out EFishingAreaType areaType, out _))
        {
            return string.Empty;
        }

        switch (areaType)
        {
            case EFishingAreaType.FreshWater:
                return "[" + key + "] " + _freshWaterPromptText;

            case EFishingAreaType.SeaWater:
                return "[" + key + "] " + _seaWaterPromptText;

            default:
                return string.Empty;
        }
    }

    private bool HasEnoughFishingHunger()
    {
        if (!_useHungerCost || _requiredHunger <= 0f)
        {
            return true;
        }

        if (DataManager.Ins == null || DataManager.Ins.Player == null)
        {
            return false;
        }

        return DataManager.Ins.Player.CurHunger >= _requiredHunger;
    }

    private bool TryConsumeFishingHunger()
    {
        if (!_useHungerCost || _requiredHunger <= 0f)
        {
            return true;
        }

        if (DataManager.Ins == null || DataManager.Ins.Player == null)
        {
            return false;
        }

        if (DataManager.Ins.Player.CurHunger < _requiredHunger)
        {
            return false;
        }

        return DataManager.Ins.Player.ConsumeHunger(_requiredHunger);
    }

    public bool TryFish(CPlayerCollector2D collector)
    {
        EFishingStartResult result = ValidateFishingStart(
            collector,
            false,
            false,
            Vector2.zero,
            out EFishingAreaType areaType,
            out Vector2 checkPos,
            out _,
            out _);

        if (result != EFishingStartResult.Success)
        {
            return HandleFishingStartFail(collector, result);
        }

        if (!TryConsumeFishingHunger())
        {
            return HandleFishingStartFail(collector, EFishingStartResult.NotEnoughHunger);
        }

        StartFishingRoutine(collector, areaType, checkPos);
        return true;
    }

    public bool CanManualFishFromSpot(CPlayerCollector2D collector, bool useSeaFishing, Vector2 spotWorldPos)
    {
        return collector != null && collector.FishingController != null;
    }

    public bool TryFishFromSpot(CPlayerCollector2D collector, bool useSeaFishing, Vector2 spotWorldPos)
    {
        EFishingStartResult result = ValidateFishingStart(
            collector,
            true,
            useSeaFishing,
            spotWorldPos,
            out EFishingAreaType areaType,
            out Vector2 checkPos,
            out _,
            out _);

        if (result != EFishingStartResult.Success)
        {
            return HandleFishingStartFail(collector, result);
        }

        if (!TryConsumeFishingHunger())
        {
            return HandleFishingStartFail(collector, EFishingStartResult.NotEnoughHunger);
        }

        StartFishingRoutine(collector, areaType, checkPos);
        return true;
    }

    public void CancelFishingByMove()
    {
        CancelFishingInternal(_moveCancelMessage, true, true);
    }

    private EFishingStartResult ValidateFishingStart(
        CPlayerCollector2D collector,
        bool useSpotArea,
        bool useSeaFishing,
        Vector2 spotWorldPos,
        out EFishingAreaType areaType,
        out Vector2 checkPos,
        out ToolItemSO activeFishingRod,
        out BaitItemSO previewBait)
    {
        areaType = EFishingAreaType.None;
        checkPos = Vector2.zero;
        activeFishingRod = null;
        previewBait = null;

        if (_isFishing)
        {
            return EFishingStartResult.AlreadyFishing;
        }

        if (collector == null)
        {
            return EFishingStartResult.CollectorNull;
        }

        if (collector.IsBusy || !collector.CanStartInteraction())
        {
            return EFishingStartResult.CollectorBusy;
        }

        if (Time.time < _cooldownEndTime)
        {
            return EFishingStartResult.Cooldown;
        }

        if (!HasEnoughFishingHunger())
        {
            return EFishingStartResult.NotEnoughHunger;
        }

        if (_database == null || DatabaseManager.Ins == null)
        {
            return EFishingStartResult.DatabaseMissing;
        }

        if (!TryGetPlayerInventory(out Inventory playerInventory))
        {
            return EFishingStartResult.InventoryMissing;
        }

        if (!TryResolveFishingArea(
                collector,
                useSpotArea,
                useSeaFishing,
                spotWorldPos,
                out areaType,
                out checkPos))
        {
            return EFishingStartResult.NoFishingArea;
        }

        if (!IsFishingItemRequirementBypassed())
        {
            EFishingStartResult rodResult = TryResolveActiveFishingRod(playerInventory, areaType, out activeFishingRod);
            if (rodResult != EFishingStartResult.Success)
            {
                return rodResult;
            }

            EFishingStartResult baitResult = ValidateBaitStartRequirement(playerInventory);
            if (baitResult != EFishingStartResult.Success)
            {
                return baitResult;
            }
        }
        else if (_logEnabled)
        {
            Debug.Log("[FishingController] 테스트 모드: 낚싯대 / 미끼 보유 검사 우회");
        }

        bool blockedByRod = false;
        bool blockedByBait = false;

        List<SheetItemRow> candidates = GetFishCandidates(areaType, activeFishingRod, playerInventory, out blockedByRod, out blockedByBait);

        if (candidates.Count <= 0)
        {
            if (blockedByRod)
            {
                return EFishingStartResult.NoCatchableFishForQuickSlotRod;
            }

            if (blockedByBait)
            {
                return EFishingStartResult.NoCatchableFishForBait;
            }

            return EFishingStartResult.NoCandidateFish;
        }

        bool hasValidRewardItemData = false;

        for (int i = 0; i < candidates.Count; i++)
        {
            SheetItemRow row = candidates[i];
            if (row == null)
            {
                continue;
            }

            ItemSO rewardItemSo = DatabaseManager.Ins.Item(row.id);
            if (rewardItemSo == null)
            {
                continue;
            }

            hasValidRewardItemData = true;

            if (CanInventoryAcceptItem(playerInventory, rewardItemSo, _amount))
            {
                ERarity fishRarity = ParseFishRarity(row.rarity);

                if (!TryGetUsableBaitForFish(playerInventory, areaType, fishRarity, activeFishingRod, out previewBait))
                {
                    if (_requireBait && !IsFishingItemRequirementBypassed())
                    {
                        return EFishingStartResult.NoCatchableFishForBait;
                    }

                    return EFishingStartResult.Success;
                }

                return EFishingStartResult.Success;
            }
        }

        if (!hasValidRewardItemData)
        {
            return EFishingStartResult.RewardItemDataMissing;
        }

        return EFishingStartResult.InventoryFull;
    }

    private void StartFishingRoutine(CPlayerCollector2D collector, EFishingAreaType areaType, Vector2 checkPos)
    {
        if (collector == null)
        {
            return;
        }

        _isFishing = true;
        _activeCollector = collector;
        _fishingStartPosition = collector.transform.position;

        _activeCollector.SetInteractionBusy(true);

        Vector2 fishingDirection = GetFishingDirectionFromTarget(collector, checkPos);
        ApplyFishingFacingDirection(fishingDirection);

        _activeCollector.PlayFishingAnimation();

        SubscribeMoveCancel();

        Vector2 motionTargetPos = GetFishingMotionTargetPosition(collector, checkPos, fishingDirection);
        float motionDuration = Mathf.Max(0f, _fishingMotionDuration);

        if (_logEnabled)
        {
            Debug.Log("[FishingController] OnPlayerFishing.Publish / pos = " + motionTargetPos +
                      " / duration = " + motionDuration +
                      " / isSuccess = " + _motionStartIsSuccessValue +
                      " / fishingDirection = " + fishingDirection);
        }

        OnPlayerFishing.Publish(motionTargetPos, motionDuration, _motionStartIsSuccessValue);
        RaiseFishingStartedFeedback();

        if (_logEnabled)
        {
            Debug.Log("[FishingController] 낚시 시작 / areaType = " + areaType + " / checkPos = " + checkPos);
        }

        _fishingRoutine = StartCoroutine(CoFishing(collector, areaType));
    }

    private IEnumerator CoFishing(CPlayerCollector2D collector, EFishingAreaType areaType)
    {
        if (_fishingDelay > 0f)
        {
            yield return new WaitForSeconds(_fishingDelay);
        }

        if (HasCollectorMovedSinceStart(collector))
        {
            CancelFishingInternal(_moveCancelMessage, true, true);
            yield break;
        }

        if (!TryGetPlayerInventory(out Inventory playerInventory))
        {
            HandleFishingRuntimeFail("플레이어 인벤토리를 찾지 못했습니다.");
            yield break;
        }

        ToolItemSO activeFishingRod = null;
        if (!IsFishingItemRequirementBypassed())
        {
            TryResolveActiveFishingRod(playerInventory, areaType, out activeFishingRod);
        }

        BaitItemSO selectedBait = null;
        SheetItemRow fishRow = GetRandomFishRow(areaType, playerInventory, activeFishingRod, out selectedBait);

        if (fishRow == null)
        {
            HandleFishingRuntimeFail("낚시 결과를 획득하지 못했습니다.");
            yield break;
        }

        ItemSO rewardItemSo = DatabaseManager.Ins != null ? DatabaseManager.Ins.Item(fishRow.id) : null;
        if (rewardItemSo == null)
        {
            HandleFishingRuntimeFail("물고기 데이터를 찾지 못했습니다.");
            yield break;
        }

        if (!CanInventoryAcceptItem(playerInventory, rewardItemSo, _amount))
        {
            FailFishingWithResult(EFishingStartResult.InventoryFull, "인벤토리가 가득 찼습니다.", true);
            yield break;
        }

        if (_requireBait && !IsFishingItemRequirementBypassed())
        {
            if (selectedBait == null)
            {
                FailFishingWithResult(EFishingStartResult.NoCatchableFishForBait, "현재 스폿에 맞는 미끼가 없거나 미끼 등급이 맞지 않습니다.", true);
                yield break;
            }

            if (_consumeBaitOnSuccess)
            {
                bool removed = playerInventory.TryRemoveItem(selectedBait.Id, Mathf.Max(1, _requiredBaitCount));
                if (!removed)
                {
                    FailFishingWithResult(EFishingStartResult.BaitInsufficient, "미끼가 부족합니다.", true);
                    yield break;
                }
            }
        }

        bool received = collector != null && collector.TryReceiveItem(fishRow.id, _amount);

        if (!received)
        {
            FailFishingWithResult(EFishingStartResult.InventoryFull, "인벤토리가 가득 찼습니다.", true);
            yield break;
        }

        if (_fishingSkillExp > 0 && collector != null)
        {
            collector.TryAddLifeSkillExp(ELifeSkill.Fishing, _fishingSkillExp);
        }

        if (_logEnabled)
        {
            Debug.Log("[FishingController] 낚시 성공: " + fishRow.name + " (" + fishRow.id + ") x" + _amount);
        }

        OnPlayerFishCaught.Publish(fishRow.id);

        QueueCatchSuccessFeedback(fishRow != null ? fishRow.name : string.Empty, _amount);

        RaiseFishingSucceededFeedback();

        ReleaseFishingState(true);
    }

    private void HandleFishingRuntimeFail(string message)
    {
        FailFishingWithCustomMessage(message, true);
    }

    private bool HandleFishingStartFail(CPlayerCollector2D collector, EFishingStartResult result)
    {
        InteractionFeedbackRelay.ClearQueuedFishingSuccessMessage();
        _lastFailMessage = GetFailMessage(result);

        if (_logEnabled)
        {
            Debug.LogWarning("[FishingController] 낚시 시작 실패: " + _lastFailMessage);
        }

        NotifyFeedbackByResult(result, _lastFailMessage);
        return false;
    }


    private void FailFishingWithResult(EFishingStartResult result, string fallbackMessage, bool applyCooldown)
    {
        InteractionFeedbackRelay.ClearQueuedFishingSuccessMessage();

        _lastFailMessage = string.IsNullOrWhiteSpace(fallbackMessage)
            ? GetFailMessage(result)
            : fallbackMessage;

        if (_logEnabled)
        {
            Debug.LogWarning("[FishingController] 낚시 실패: " + _lastFailMessage);
        }

        NotifyFeedbackByResult(result, _lastFailMessage);
        ReleaseFishingState(applyCooldown);
    }

    private void FailFishingWithCustomMessage(string message, bool applyCooldown)
    {
        InteractionFeedbackRelay.ClearQueuedFishingSuccessMessage();

        _lastFailMessage = string.IsNullOrWhiteSpace(message)
            ? "낚시에 실패했습니다."
            : message;

        if (_logEnabled)
        {
            Debug.LogWarning("[FishingController] 낚시 실패: " + _lastFailMessage);
        }

        if (_feedbackRelay != null)
        {
            _feedbackRelay.ShowFailureFeedback(_lastFailMessage);
        }
        else
        {
            NotifyFallbackFeedback(_lastFailMessage);
        }

        ReleaseFishingState(applyCooldown);
    }

    private void RaiseFishingStartedFeedback()
    {
        if (_feedbackRelay != null)
        {
            _feedbackRelay.OnFishingStarted();
            return;
        }

        _onFishingStarted?.Invoke();
    }

    private void RaiseFishingSucceededFeedback()
    {
        if (_feedbackRelay != null)
        {
            _feedbackRelay.OnFishingSucceeded();
            return;
        }

        _onFishingSucceeded?.Invoke();
    }

    private void RaiseFishingCanceledFeedback(string message)
    {
        if (_feedbackRelay != null)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _feedbackRelay.ShowWarningFeedback(message);
            }
            else
            {
                _feedbackRelay.OnFishingCanceled();
            }

            return;
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            NotifyFallbackFeedback(message);
            return;
        }

        _onFishingCanceled?.Invoke();
    }

    private void ReleaseFishingState(bool applyCooldown)
    {
        if (applyCooldown)
        {
            _cooldownEndTime = Time.time + Mathf.Max(0f, _cooldownAfterFishing);
        }

        if (_activeCollector != null)
        {
            _activeCollector.SetInteractionBusy(false);
        }

        UnsubscribeMoveCancel();

        _fishingRoutine = null;
        _activeCollector = null;
        _fishingStartPosition = Vector2.zero;
        _isFishing = false;
    }

    private void CancelFishingInternal(string message, bool showFeedback, bool publishMotionCancel)
    {
        if (!_isFishing)
        {
            return;
        }

        InteractionFeedbackRelay.ClearQueuedFishingSuccessMessage();

        if (_fishingRoutine != null)
        {
            StopCoroutine(_fishingRoutine);
            _fishingRoutine = null;
        }

        if (_logEnabled)
        {
            Debug.Log("[FishingController] 낚시 취소: " + message);
        }

        if (showFeedback)
        {
            RaiseFishingCanceledFeedback(message);
        }

        if (publishMotionCancel && _logEnabled)
        {
            Debug.Log("[FishingController] OnPlayerCanceled.Publish");
        }

        OnPlayerCanceled.Publish();
        ReleaseFishingState(false);
    }

    private void OnPlayerMoveEvent(OnPlayerMove eventData)
    {
        if (!_isFishing)
        {
            return;
        }

        if (eventData.moved.sqrMagnitude <= _moveCancelInputThreshold)
        {
            return;
        }

        CancelFishingByMove();
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

    private bool TryResolveFishingArea(
        CPlayerCollector2D collector,
        bool useSpotArea,
        bool useSeaFishing,
        Vector2 spotWorldPos,
        out EFishingAreaType areaType,
        out Vector2 checkPos)
    {
        areaType = EFishingAreaType.None;
        checkPos = Vector2.zero;

        if (useSpotArea)
        {
            areaType = useSeaFishing ? EFishingAreaType.SeaWater : EFishingAreaType.FreshWater;
            checkPos = spotWorldPos;
            return true;
        }

        return TryGetFishingAreaType(collector, out areaType, out checkPos);
    }

    private bool TryGetPlayerInventory(out Inventory inventory)
    {
        inventory = null;

        if (InventoryManager.Ins == null)
        {
            return false;
        }

        inventory = InventoryManager.Ins.PlayerInventory;
        if (inventory == null)
        {
            return false;
        }

        InventorySlot[] slots = inventory.InventorySlots;
        if (slots == null || slots.Length <= 0)
        {
            return false;
        }

        return true;
    }

    private Vector2 GetFishingMotionTargetPosition(CPlayerCollector2D collector, Vector2 fallbackPos, Vector2 fishingDirection)
    {
        if (collector == null)
        {
            return fallbackPos + _motionTargetOffset;
        }

        if (_useFacingDirectionMotionTarget)
        {
            Vector2 dir = fishingDirection;

            if (dir.sqrMagnitude <= 0.0001f)
            {
                dir = Vector2.down;
            }

            Vector2 playerPos = collector.transform.position;
            return playerPos + dir * Mathf.Max(0f, _motionTargetDistance) + _motionTargetOffset;
        }

        return fallbackPos + _motionTargetOffset;
    }

    private Vector2 GetFishingDirectionFromTarget(CPlayerCollector2D collector, Vector2 targetPos)
    {
        if (collector == null)
        {
            return Vector2.down;
        }

        Vector2 playerPos = collector.transform.position;
        Vector2 delta = targetPos - playerPos;

        if (delta.sqrMagnitude <= 0.0001f)
        {
            return GetCardinalFacingDirection();
        }

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            return delta.x >= 0f ? Vector2.right : Vector2.left;
        }

        return delta.y >= 0f ? Vector2.up : Vector2.down;
    }

    private void ApplyFishingFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        if (DataManager.Ins != null && DataManager.Ins.Player != null)
        {
            Vector2 currentPos = _activeCollector != null
                ? (Vector2)_activeCollector.transform.position
                : DataManager.Ins.Player.Position;

            DataManager.Ins.Player.SetTransform(currentPos, direction);
        }
    }

    private bool IsFishingItemRequirementBypassed()
    {
        return _bypassRequiredFishingItemsForTest;
    }

    private EFishingStartResult TryResolveActiveFishingRod(Inventory inventory, EFishingAreaType areaType, out ToolItemSO activeFishingRod)
    {
        activeFishingRod = null;

        bool needFishingRod = _requireFishingRodInQuickSlot
            || _limitCatchByQuickSlotRodRarity
            || HasAreaRodRestriction(areaType);

        if (!needFishingRod)
        {
            return EFishingStartResult.Success;
        }

        if (!TryGetBestFishingRodInInventory(inventory, areaType, out activeFishingRod))
        {
            return EFishingStartResult.NoFishingRodInQuickSlot;
        }

        return EFishingStartResult.Success;
    }

    private bool TryGetBestFishingRodInInventory(Inventory inventory, EFishingAreaType areaType, out ToolItemSO bestRod)
    {
        bestRod = null;

        if (inventory == null || inventory.InventorySlots == null)
        {
            return false;
        }

        InventorySlot[] slots = inventory.InventorySlots;

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

            if (candidate.Type != EType.Fishingrod)
            {
                continue;
            }

            if (!IsRodAllowedInArea(areaType, candidate))
            {
                continue;
            }

            if (bestRod == null || IsBetterTool(candidate, bestRod))
            {
                bestRod = candidate;
            }
        }

        return bestRod != null;
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

        return string.Compare(candidate.Id, currentBest.Id, StringComparison.Ordinal) < 0;
    }

    private EFishingStartResult ValidateBaitStartRequirement(Inventory inventory)
    {
        if (!_requireBait)
        {
            return EFishingStartResult.Success;
        }

        int safeRequiredBaitCount = Mathf.Max(1, _requiredBaitCount);

        if (!HasAnyBaitInInventory(inventory))
        {
            return EFishingStartResult.NoBaitInInventory;
        }

        if (GetTotalBaitCount(inventory) < safeRequiredBaitCount)
        {
            return EFishingStartResult.BaitInsufficient;
        }

        return EFishingStartResult.Success;
    }

    private bool HasAnyBaitInInventory(Inventory inventory)
    {
        if (inventory == null || inventory.InventorySlots == null)
        {
            return false;
        }

        InventorySlot[] slots = inventory.InventorySlots;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                continue;
            }

            ItemSO itemSo = slots[i].ItemSO;
            if (itemSo == null)
            {
                continue;
            }

            if (itemSo.Type == EType.BaitItem)
            {
                return true;
            }
        }

        return false;
    }

    private int GetTotalBaitCount(Inventory inventory)
    {
        int totalCount = 0;

        if (inventory == null || inventory.InventorySlots == null)
        {
            return totalCount;
        }

        InventorySlot[] slots = inventory.InventorySlots;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                continue;
            }

            ItemSO itemSo = slots[i].ItemSO;
            if (itemSo == null || itemSo.Type != EType.BaitItem)
            {
                continue;
            }

            totalCount += Mathf.Max(0, slots[i].CurStack);
        }

        return totalCount;
    }

    private bool TryGetUsableBaitForFish(Inventory inventory, EFishingAreaType areaType, ERarity fishRarity, ToolItemSO activeFishingRod, out BaitItemSO selectedBait)
    {
        selectedBait = null;

        if (!_requireBait)
        {
            return true;
        }

        if (inventory == null || inventory.InventorySlots == null)
        {
            return false;
        }

        int safeRequiredBaitCount = Mathf.Max(1, _requiredBaitCount);
        InventorySlot[] slots = inventory.InventorySlots;

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];

            if (slot.IsEmpty)
            {
                continue;
            }

            BaitItemSO candidate = slot.ItemSO as BaitItemSO;
            if (candidate == null)
            {
                continue;
            }

            if (!IsBaitAllowedInArea(areaType, candidate))
            {
                continue;
            }

            if (!CanUseBaitBySeaRule(areaType, candidate))
            {
                continue;
            }

            if (activeFishingRod != null && candidate.Rarity > activeFishingRod.Rarity)
            {
                continue;
            }

            if (_limitCatchByBaitRarity && !CanBaitCatchFishRarity(candidate, fishRarity))
            {
                continue;
            }

            if (!inventory.CheckHasItem(candidate.Id, safeRequiredBaitCount))
            {
                continue;
            }

            if (selectedBait == null || IsBetterBaitToConsume(candidate, selectedBait))
            {
                selectedBait = candidate;
            }
        }

        return selectedBait != null;
    }

    private bool HasCollectorMovedSinceStart(CPlayerCollector2D collector)
    {
        if (collector == null)
        {
            return false;
        }

        Vector2 currentPos = collector.transform.position;
        float movedSqrDistance = (currentPos - _fishingStartPosition).sqrMagnitude;
        return movedSqrDistance > _moveCancelPositionThreshold;
    }

    private bool TryGetFishingAreaType(CPlayerCollector2D collector, out EFishingAreaType areaType, out Vector2 checkWorldPos)
    {
        areaType = EFishingAreaType.None;
        checkWorldPos = Vector2.zero;

        if (collector == null)
        {
            if (_logEnabled)
            {
                Debug.Log("[FishingController] TryGetFishingAreaType 실패: collector null");
            }

            return false;
        }

        Vector2 facingDir = GetCardinalFacingDirection();
        Vector2 primaryCheckPos = GetFishingCheckWorldPosition(collector, facingDir);
        Vector2 secondaryCheckPos = (Vector2)collector.transform.position + _checkOffset;
        Vector2 fallbackCheckPos = collector.transform.position;

        if (TryFindFishingSpotArea(primaryCheckPos, out areaType, out Vector2 spotWorldPos)
            || TryFindFishingSpotArea(secondaryCheckPos, out areaType, out spotWorldPos)
            || TryFindFishingSpotArea(fallbackCheckPos, out areaType, out spotWorldPos))
        {
            checkWorldPos = spotWorldPos;

            if (_logEnabled)
            {
                Debug.Log("[FishingController] 수동 낚시 스폿 판정 성공 / areaType = " + areaType +
                          " / checkWorldPos = " + checkWorldPos +
                          " / primaryCheckPos = " + primaryCheckPos +
                          " / facingDir = " + facingDir +
                          " / playerPos = " + collector.transform.position);
            }

            return true;
        }

        if (_logEnabled)
        {
            Debug.Log("[FishingController] TryGetFishingAreaType 실패: 수동 낚시 위치에서 FishingSpot을 찾지 못했습니다. " +
                      "primaryCheckPos = " + primaryCheckPos +
                      " / secondaryCheckPos = " + secondaryCheckPos +
                      " / fallbackCheckPos = " + fallbackCheckPos +
                      " / facingDir = " + facingDir +
                      " / playerPos = " + collector.transform.position);
        }

        return false;
    }

    private Vector2 GetFishingCheckWorldPosition(CPlayerCollector2D collector, Vector2 facingDir)
    {
        if (collector == null)
        {
            return Vector2.zero;
        }

        return (Vector2)collector.transform.position + _checkOffset + facingDir * Mathf.Max(0f, _checkDistance);
    }

    private bool TryFindFishingSpotArea(Vector2 worldPos, out EFishingAreaType areaType, out Vector2 spotWorldPos)
    {
        areaType = EFishingAreaType.None;
        spotWorldPos = worldPos;

        float searchRadius = Mathf.Max(0.05f, _manualSpotSearchRadius);
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, searchRadius);

        if (hits == null || hits.Length == 0)
        {
            return false;
        }

        CFishingSpotInteractObject2D nearestSpot = null;
        float nearestSqrDistance = float.MaxValue;
        Vector2 nearestPoint = worldPos;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            CFishingSpotInteractObject2D spot = hit.GetComponentInParent<CFishingSpotInteractObject2D>();
            if (spot == null)
            {
                spot = hit.GetComponent<CFishingSpotInteractObject2D>();
            }

            if (spot == null)
            {
                continue;
            }

            if (!TryConvertSpotToAreaType(spot, out EFishingAreaType candidateAreaType))
            {
                continue;
            }

            Vector2 closestPoint = hit.ClosestPoint(worldPos);
            float sqrDistance = (closestPoint - worldPos).sqrMagnitude;

            if (nearestSpot != null && sqrDistance >= nearestSqrDistance)
            {
                continue;
            }

            nearestSpot = spot;
            nearestSqrDistance = sqrDistance;
            nearestPoint = closestPoint;
            areaType = candidateAreaType;
        }

        if (nearestSpot == null)
        {
            return false;
        }

        spotWorldPos = nearestPoint;
        return true;
    }

    private bool TryConvertSpotToAreaType(CFishingSpotInteractObject2D spot, out EFishingAreaType areaType)
    {
        areaType = EFishingAreaType.None;

        if (spot == null)
        {
            return false;
        }

        if (s_fishingSpotTypeField != null)
        {
            object rawValue = s_fishingSpotTypeField.GetValue(spot);
            if (rawValue != null)
            {
                string spotTypeName = rawValue.ToString();

                if (string.Equals(spotTypeName, "SeaWater", StringComparison.OrdinalIgnoreCase))
                {
                    areaType = EFishingAreaType.SeaWater;
                    return true;
                }

                if (string.Equals(spotTypeName, "FreshWater", StringComparison.OrdinalIgnoreCase))
                {
                    areaType = EFishingAreaType.FreshWater;
                    return true;
                }
            }
        }

        string message = spot.GetMessage();
        if (!string.IsNullOrWhiteSpace(message) && message.IndexOf("바다", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            areaType = EFishingAreaType.SeaWater;
            return true;
        }

        areaType = EFishingAreaType.FreshWater;
        return true;
    }

    private Vector2 GetCardinalFacingDirection()
    {
        Vector2 dir = Vector2.down;

        if (DataManager.Ins != null && DataManager.Ins.Player != null)
        {
            dir = DataManager.Ins.Player.Direction;
        }

        if (dir.sqrMagnitude <= 0.0001f)
        {
            return Vector2.down;
        }

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x >= 0f ? Vector2.right : Vector2.left;
        }

        return dir.y >= 0f ? Vector2.up : Vector2.down;
    }

    private List<SheetItemRow> GetFishCandidates(
        EFishingAreaType areaType,
        ToolItemSO activeFishingRod,
        Inventory inventory,
        out bool blockedByRod,
        out bool blockedByBait)
    {
        List<SheetItemRow> result = new List<SheetItemRow>();
        blockedByRod = false;
        blockedByBait = false;

        if (_database == null)
        {
            return result;
        }

        IReadOnlyList<SheetItemRow> allItems = _database.AllItems;

        for (int i = 0; i < allItems.Count; i++)
        {
            SheetItemRow row = allItems[i];

            if (row == null)
            {
                continue;
            }

            if (!string.Equals(row.category, "Fish", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!IsFishRowAllowedInArea(row, areaType))
            {
                continue;
            }

            ERarity fishRarity = ParseFishRarity(row.rarity);

            if (_limitCatchByQuickSlotRodRarity && activeFishingRod != null && fishRarity > activeFishingRod.Rarity)
            {
                blockedByRod = true;
                continue;
            }

            if (_requireBait && !IsFishingItemRequirementBypassed())
            {
                if (!TryGetUsableBaitForFish(inventory, areaType, fishRarity, activeFishingRod, out _))
                {
                    blockedByBait = true;
                    continue;
                }
            }

            result.Add(row);
        }

        return result;
    }

    private SheetItemRow GetRandomFishRow(
        EFishingAreaType areaType,
        Inventory playerInventory,
        ToolItemSO activeFishingRod,
        out BaitItemSO selectedBait)
    {
        selectedBait = null;

        bool blockedByRod = false;
        bool blockedByBait = false;

        List<SheetItemRow> candidates = GetFishCandidates(
            areaType,
            activeFishingRod,
            playerInventory,
            out blockedByRod,
            out blockedByBait);

        if (candidates.Count == 0)
        {
            return null;
        }

        List<SheetItemRow> validCandidates = new List<SheetItemRow>();

        for (int i = 0; i < candidates.Count; i++)
        {
            SheetItemRow row = candidates[i];
            if (row == null)
            {
                continue;
            }

            if (DatabaseManager.Ins == null)
            {
                continue;
            }

            ItemSO rewardItemSo = DatabaseManager.Ins.Item(row.id);
            if (rewardItemSo == null)
            {
                continue;
            }

            validCandidates.Add(row);
        }

        if (validCandidates.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;

        for (int i = 0; i < validCandidates.Count; i++)
        {
            totalWeight += Mathf.Max(0.01f, GetWeightByRarity(validCandidates[i].rarity));
        }

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < validCandidates.Count; i++)
        {
            cumulative += Mathf.Max(0.01f, GetWeightByRarity(validCandidates[i].rarity));

            if (randomValue <= cumulative)
            {
                ERarity fishRarity = ParseFishRarity(validCandidates[i].rarity);
                TryGetUsableBaitForFish(playerInventory, areaType, fishRarity, activeFishingRod, out selectedBait);
                return validCandidates[i];
            }
        }

        ERarity lastFishRarity = ParseFishRarity(validCandidates[validCandidates.Count - 1].rarity);
        TryGetUsableBaitForFish(playerInventory, areaType, lastFishRarity, activeFishingRod, out selectedBait);

        return validCandidates[validCandidates.Count - 1];
    }

    private bool IsFishRowAllowedInArea(SheetItemRow row, EFishingAreaType areaType)
    {
        if (row == null)
        {
            return false;
        }

        switch (areaType)
        {
            case EFishingAreaType.FreshWater:
                if (!row.isWaterFish)
                {
                    return false;
                }
                break;

            case EFishingAreaType.SeaWater:
                if (!row.isDeepWaterFish)
                {
                    return false;
                }
                break;

            default:
                return false;
        }

        ERarity fishRarity = ParseFishRarity(row.rarity);
        return IsFishRarityAllowedInArea(areaType, fishRarity);
    }

    private bool IsFishRarityAllowedInArea(EFishingAreaType areaType, ERarity fishRarity)
    {
        switch (areaType)
        {
            case EFishingAreaType.FreshWater:
                return fishRarity == ERarity.Basic
                    || fishRarity == ERarity.Solid;

            case EFishingAreaType.SeaWater:
                return fishRarity == ERarity.Superior
                    || fishRarity == ERarity.Prime
                    || fishRarity == ERarity.Masterwork;

            default:
                return false;
        }
    }

    private bool CanBaitCatchFishRarity(BaitItemSO baitItem, ERarity fishRarity)
    {
        if (baitItem == null)
        {
            return false;
        }

        ERarity[] catchableRarities = GetCatchableFishRarities(baitItem);
        if (catchableRarities == null || catchableRarities.Length == 0)
        {
            return baitItem.Rarity >= fishRarity;
        }

        for (int i = 0; i < catchableRarities.Length; i++)
        {
            if (catchableRarities[i] == fishRarity)
            {
                return true;
            }
        }

        return false;
    }

    private ERarity[] GetCatchableFishRarities(BaitItemSO baitItem)
    {
        if (baitItem == null || s_baitCatchFishRarityField == null)
        {
            return null;
        }

        return s_baitCatchFishRarityField.GetValue(baitItem) as ERarity[];
    }

    private bool IsBetterBaitToConsume(BaitItemSO candidate, BaitItemSO currentSelected)
    {
        if (candidate == null)
        {
            return false;
        }

        if (currentSelected == null)
        {
            return true;
        }

        if (candidate.Rarity != currentSelected.Rarity)
        {
            return candidate.Rarity < currentSelected.Rarity;
        }

        return string.Compare(candidate.Id, currentSelected.Id, StringComparison.Ordinal) < 0;
    }

    private bool IsRodAllowedInArea(EFishingAreaType areaType, ToolItemSO rod)
    {
        if (rod == null)
        {
            return false;
        }

        string[] allowedIds = GetAllowedRodIds(areaType);
        if (allowedIds == null || allowedIds.Length == 0)
        {
            return true;
        }

        return ContainsItemId(allowedIds, rod.Id);
    }

    private bool IsBaitAllowedInArea(EFishingAreaType areaType, BaitItemSO bait)
    {
        if (bait == null)
        {
            return false;
        }

        string[] allowedIds = GetAllowedBaitIds(areaType);
        if (allowedIds == null || allowedIds.Length == 0)
        {
            return true;
        }

        return ContainsItemId(allowedIds, bait.Id);
    }

    private bool CanUseBaitBySeaRule(EFishingAreaType areaType, BaitItemSO bait)
    {
        if (bait == null)
        {
            return false;
        }

        if (areaType != EFishingAreaType.SeaWater)
        {
            return true;
        }

        if (bait.CanUseSea)
        {
            return true;
        }

        return ContainsItemId(_seaWaterAllowedBaitIds, bait.Id);
    }

    private bool HasAreaRodRestriction(EFishingAreaType areaType)
    {
        string[] allowedIds = GetAllowedRodIds(areaType);
        return allowedIds != null && allowedIds.Length > 0;
    }

    private string[] GetAllowedRodIds(EFishingAreaType areaType)
    {
        switch (areaType)
        {
            case EFishingAreaType.FreshWater:
                return _freshWaterAllowedRodIds;

            case EFishingAreaType.SeaWater:
                return _seaWaterAllowedRodIds;

            default:
                return null;
        }
    }

    private string[] GetAllowedBaitIds(EFishingAreaType areaType)
    {
        switch (areaType)
        {
            case EFishingAreaType.FreshWater:
                return _freshWaterAllowedBaitIds;

            case EFishingAreaType.SeaWater:
                return _seaWaterAllowedBaitIds;

            default:
                return null;
        }
    }

    private bool ContainsItemId(string[] candidates, string itemId)
    {
        if (candidates == null || candidates.Length == 0 || string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        for (int i = 0; i < candidates.Length; i++)
        {
            string candidate = candidates[i];

            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (string.Equals(candidate.Trim(), itemId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private float GetWeightByRarity(string rarity)
    {
        ERarity parsed = ParseFishRarity(rarity);

        switch (parsed)
        {
            case ERarity.Basic:
                return _basicWeight;

            case ERarity.Solid:
                return _solidWeight;

            case ERarity.Superior:
                return _superiorWeight;

            case ERarity.Prime:
                return _primeWeight;

            case ERarity.Masterwork:
                return _masterworkWeight;

            default:
                return _unknownWeight;
        }
    }

    private ERarity ParseFishRarity(string rarityText)
    {
        if (string.IsNullOrWhiteSpace(rarityText))
        {
            return ERarity.Basic;
        }

        string key = rarityText.Trim().ToLowerInvariant();

        switch (key)
        {
            case "basic":
            case "기본":
                return ERarity.Basic;

            case "solid":
            case "견고":
                return ERarity.Solid;

            case "rare":
            case "superior":
            case "고급":
            case "희귀":
                return ERarity.Superior;

            case "prime":
            case "프라임":
                return ERarity.Prime;

            case "legend":
            case "legendary":
            case "master":
            case "masterwork":
            case "masterwark":
            case "전설":
            case "마스터":
            case "마스터워크":
                return ERarity.Masterwork;

            default:
                return ERarity.Basic;
        }
    }

    private bool CanInventoryAcceptItem(Inventory inventory, ItemSO itemData, int amount)
    {
        if (inventory == null)
        {
            return false;
        }

        if (itemData == null)
        {
            return false;
        }

        if (amount <= 0)
        {
            return false;
        }

        InventorySlot[] slots = inventory.InventorySlots;
        if (slots == null || slots.Length == 0)
        {
            return false;
        }

        int remainAmount = amount;
        int maxStack = Mathf.Max(1, itemData.MaxStack);

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                continue;
            }

            if (slots[i].ItemSO == null)
            {
                continue;
            }

            if (slots[i].ItemSO.Id != itemData.Id)
            {
                continue;
            }

            int remainStack = maxStack - slots[i].CurStack;
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
            if (!slots[i].IsEmpty)
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

    private string GetFailMessage(EFishingStartResult result)
    {
        switch (result)
        {
            case EFishingStartResult.AlreadyFishing:
                return "이미 낚시 중입니다.";

            case EFishingStartResult.CollectorNull:
                return "플레이어를 찾지 못했습니다.";

            case EFishingStartResult.CollectorBusy:
                return "다른 상호작용 진행 중입니다.";

            case EFishingStartResult.Cooldown:
                return "잠시 후 다시 시도해주세요.";

            case EFishingStartResult.NotEnoughHunger:
                return _notEnoughHungerMessage;

            case EFishingStartResult.DatabaseMissing:
                return "낚시 데이터가 연결되지 않았습니다.";

            case EFishingStartResult.InventoryMissing:
                return "플레이어 인벤토리를 찾지 못했습니다.";

            case EFishingStartResult.NoFishingArea:
                return "이 위치에서는 낚시할 수 없습니다.";

            case EFishingStartResult.NoFishingRodInQuickSlot:
            case EFishingStartResult.NoFishingRodInInventory:
                return "낚싯대가 없습니다.";

            case EFishingStartResult.NoBaitInInventory:
                return "미끼가 없습니다.";

            case EFishingStartResult.BaitInsufficient:
                return "미끼가 부족합니다.";

            case EFishingStartResult.NoCatchableFishForQuickSlotRod:
                return "낚싯대 등급이 낮습니다.";

            case EFishingStartResult.NoCatchableFishForBait:
                return "현재 스폿에 맞는 미끼가 없거나 미끼 등급이 맞지 않습니다.";

            case EFishingStartResult.NoCandidateFish:
                return "이 지역 물고기가 없습니다.";

            case EFishingStartResult.RewardItemDataMissing:
                return "물고기 데이터를 찾지 못했습니다.";

            case EFishingStartResult.InventoryFull:
                return "인벤토리가 가득 찼습니다.";

            default:
                return "낚시를 시작할 수 없습니다.";
        }
    }

    private void NotifyFeedbackByResult(EFishingStartResult result, string fallbackMessage)
    {
        if (_feedbackRelay != null)
        {
            switch (result)
            {
                case EFishingStartResult.AlreadyFishing:
                case EFishingStartResult.CollectorBusy:
                case EFishingStartResult.Cooldown:
                case EFishingStartResult.NoFishingArea:
                case EFishingStartResult.NotEnoughHunger:
                    _feedbackRelay.ShowWarningFeedback(fallbackMessage);
                    return;

                case EFishingStartResult.NoBaitInInventory:
                    _feedbackRelay.OnFishingNoBait();
                    return;

                case EFishingStartResult.NoFishingRodInQuickSlot:
                case EFishingStartResult.NoFishingRodInInventory:
                    _feedbackRelay.OnFishingNoRod();
                    return;

                case EFishingStartResult.BaitInsufficient:
                    _feedbackRelay.OnFishingBaitInsufficient();
                    return;

                case EFishingStartResult.NoCatchableFishForQuickSlotRod:
                    _feedbackRelay.OnFishingToolRarityLow();
                    return;

                case EFishingStartResult.NoCatchableFishForBait:
                    _feedbackRelay.OnFishingBaitRarityLow();
                    return;

                case EFishingStartResult.InventoryFull:
                    _feedbackRelay.OnInventoryFull();
                    return;

                case EFishingStartResult.CollectorNull:
                case EFishingStartResult.DatabaseMissing:
                case EFishingStartResult.InventoryMissing:
                case EFishingStartResult.NoCandidateFish:
                case EFishingStartResult.RewardItemDataMissing:
                    _feedbackRelay.ShowFailureFeedback(fallbackMessage);
                    return;

                default:
                    _feedbackRelay.OnFishingFailed();
                    return;
            }
        }

        NotifyFallbackFeedback(fallbackMessage);
    }

    private void NotifyFallbackFeedback(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (_logEnabled)
        {
            Debug.Log("[FishingController][Feedback] " + message);
        }

        _onFeedbackMessage?.Invoke(message);
    }

    private void QueueCatchSuccessFeedback(string itemName, int amount)
    {
        if (!_showCatchSuccessFeedback)
        {
            InteractionFeedbackRelay.ClearQueuedFishingSuccessMessage();
            return;
        }

        string message = BuildCatchSuccessMessage(itemName, amount);

        if (_feedbackRelay != null)
        {
            InteractionFeedbackRelay.QueueFishingSuccessMessageText(message);
            return;
        }

        NotifyFallbackFeedback(message);
    }

    private string BuildCatchSuccessMessage(string itemName, int amount)
    {
        string safeName = string.IsNullOrWhiteSpace(itemName) ? "물고기" : itemName.Trim();
        int safeAmount = Mathf.Max(1, amount);

        try
        {
            if (safeAmount > 1)
            {
                string format = string.IsNullOrWhiteSpace(_fishCatchSuccessMultipleFormat)
                    ? "{0} x{1} 획득"
                    : _fishCatchSuccessMultipleFormat;

                return string.Format(format, safeName, safeAmount);
            }

            string singleFormat = string.IsNullOrWhiteSpace(_fishCatchSuccessSingleFormat)
                ? "{0} 획득"
                : _fishCatchSuccessSingleFormat;

            return string.Format(singleFormat, safeName);
        }
        catch (FormatException)
        {
            if (safeAmount > 1)
            {
                return safeName + " x" + safeAmount + " 획득";
            }

            return safeName + " 획득";
        }
    }

    private void OnDisable()
    {
        InteractionFeedbackRelay.ClearQueuedFishingSuccessMessage();
        CancelFishingInternal(string.Empty, false, false);
    }

    private void Start()
    {
        StartCoroutine(CoFindSheetItemDatabase());
    }

    private IEnumerator CoFindSheetItemDatabase()
    {
        while (_database == null)
        {
            _database = FindAnyObjectByType<SheetItemDatabase>();
            yield return null;
        }
    }
}
