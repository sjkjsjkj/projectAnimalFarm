using System;
using System.Collections;
using System.Collections.Generic;
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

    public string LastFailMessage => _lastFailMessage;

    /// <summary>
    /// FishingSpot에서 CanInteract 단계에서 너무 일찍 막아버리면
    /// 실패 메시지를 띄울 기회가 없으므로 여기서는 느슨하게 허용한다.
    /// 실제 성공/실패 판정은 TryFish에서 한다.
    /// </summary>
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

        StartFishingRoutine(collector, areaType, checkPos);
        return true;
    }

    /// <summary>
    /// FishingSpot의 CanInteract에서 실패 메시지를 막지 않기 위해
    /// 여기서는 느슨하게 허용한다.
    /// </summary>
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
            EFishingStartResult rodResult = TryResolveActiveFishingRod(playerInventory, out activeFishingRod);
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

                if (!TryGetUsableBaitForFish(playerInventory, areaType, fishRarity, out previewBait))
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
        _onFishingStarted?.Invoke();

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
            TryResolveActiveFishingRod(playerInventory, out activeFishingRod);
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

        // 중요:
        // 낚시 결과 선택 자체는 인벤토리 상태에 의해 왜곡되지 않게 유지한다.
        // 대신 실제 지급 직전에 현재 인벤토리가 선택된 물고기를 받을 수 있는지 다시 검사한다.
        // 이렇게 해야 인벤토리가 꽉 찬 상태에서 "이미 들고 있는 물고기만 계속 낚이는" 현상을 막을 수 있다.
        if (!CanInventoryAcceptItem(playerInventory, rewardItemSo, _amount))
        {
            NotifyFeedbackByResult(EFishingStartResult.InventoryFull, "인벤토리가 가득 찼습니다.");
            _onFishingFailed?.Invoke();
            ReleaseFishingState(true);
            yield break;
        }

        if (_requireBait && !IsFishingItemRequirementBypassed())
        {
            if (selectedBait == null)
            {
                NotifyFeedbackByResult(EFishingStartResult.NoCatchableFishForBait, "미끼 등급이 낮습니다.");
                _onFishingFailed?.Invoke();
                ReleaseFishingState(true);
                yield break;
            }

            if (_consumeBaitOnSuccess)
            {
                bool removed = playerInventory.TryRemoveItem(selectedBait.Id, Mathf.Max(1, _requiredBaitCount));
                if (!removed)
                {
                    NotifyFeedbackByResult(EFishingStartResult.BaitInsufficient, "미끼가 부족합니다.");
                    _onFishingFailed?.Invoke();
                    ReleaseFishingState(true);
                    yield break;
                }
            }
        }

        bool received = collector != null && collector.TryReceiveItem(fishRow.id, _amount);

        if (!received)
        {
            NotifyFeedbackByResult(EFishingStartResult.InventoryFull, "인벤토리가 가득 찼습니다.");
            _onFishingFailed?.Invoke();
            ReleaseFishingState(true);
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

        _onFishingSucceeded?.Invoke();
        ReleaseFishingState(true);
    }

    private void HandleFishingRuntimeFail(string message)
    {
        _lastFailMessage = message;

        if (_logEnabled)
        {
            Debug.LogWarning("[FishingController] 낚시 실패: " + message);
        }

        NotifyFallbackFeedback(message);
        _onFishingFailed?.Invoke();
        ReleaseFishingState(true);
    }

    private bool HandleFishingStartFail(CPlayerCollector2D collector, EFishingStartResult result)
    {
        _lastFailMessage = GetFailMessage(result);

        if (_logEnabled)
        {
            Debug.LogWarning("[FishingController] 낚시 시작 실패: " + _lastFailMessage);
        }

        NotifyFeedbackByResult(result, _lastFailMessage);
        _onFishingFailed?.Invoke();
        return false;
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
            NotifyFallbackFeedback(message);
        }

        if (publishMotionCancel && _logEnabled)
        {
            Debug.Log("[FishingController] OnPlayerCanceled.Publish");
        }

        OnPlayerCanceled.Publish();
        _onFishingCanceled?.Invoke();
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

    private EFishingStartResult TryResolveActiveFishingRod(Inventory inventory, out ToolItemSO activeFishingRod)
    {
        activeFishingRod = null;

        if (!_requireFishingRodInQuickSlot)
        {
            return EFishingStartResult.Success;
        }

        if (!TryGetBestFishingRodInInventory(inventory, out activeFishingRod))
        {
            return EFishingStartResult.NoFishingRodInQuickSlot;
        }

        return EFishingStartResult.Success;
    }

    private bool TryGetBestFishingRodInInventory(Inventory inventory, out ToolItemSO bestRod)
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

    private bool TryGetUsableBaitForFish(Inventory inventory, EFishingAreaType areaType, ERarity fishRarity, out BaitItemSO selectedBait)
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

            if (areaType == EFishingAreaType.SeaWater && !candidate.CanUseSea)
            {
                continue;
            }

            if (_limitCatchByBaitRarity && candidate.Rarity < fishRarity)
            {
                continue;
            }

            if (!inventory.CheckHasItem(candidate.Id, safeRequiredBaitCount))
            {
                continue;
            }

            if (selectedBait == null || candidate.Rarity > selectedBait.Rarity)
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
        /*areaType = EFishingAreaType.None;
        checkWorldPos = Vector2.zero;

        if (collector == null)
        {
            if (_logEnabled) Debug.Log("[FishingController] TryGetFishingAreaType 실패: collector null");
            return false;
        }

        if (TileManager.Ins == null)
        {
            if (_logEnabled) Debug.Log("[FishingController] TryGetFishingAreaType 실패: TileManager.Ins null");
            return false;
        }

        if (TileManager.Ins.Tile == null)
        {
            if (_logEnabled) Debug.Log("[FishingController] TryGetFishingAreaType 실패: TileManager.Ins.Tile null");
            return false;
        }

        TileMap map = TileManager.Ins.Tile;
        Vector2 facingDir = GetCardinalFacingDirection();
        checkWorldPos = (Vector2)collector.transform.position + _checkOffset + facingDir * _checkDistance;

        if (_logEnabled)
        {
            Debug.Log("[FishingController] 판정 위치 = " + checkWorldPos +
                      " / facingDir = " + facingDir +
                      " / fresh = " + fresh +
                      " / sea = " + sea +
                      " / playerPos = " + collector.transform.position);
        }

        if (fresh)
        {
            areaType = EFishingAreaType.FreshWater;
            return true;
        }

        if (sea)
        {
            areaType = EFishingAreaType.SeaWater;
            return true;
        }*/
        checkWorldPos = Vector2.zero;
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

            switch (areaType)
            {
                case EFishingAreaType.FreshWater:
                    if (!row.isWaterFish)
                    {
                        continue;
                    }
                    break;

                case EFishingAreaType.SeaWater:
                    if (!row.isDeepWaterFish)
                    {
                        continue;
                    }
                    break;

                default:
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
                if (!TryGetUsableBaitForFish(inventory, areaType, fishRarity, out _))
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
                TryGetUsableBaitForFish(playerInventory, areaType, fishRarity, out selectedBait);
                return validCandidates[i];
            }
        }

        ERarity lastFishRarity = ParseFishRarity(validCandidates[validCandidates.Count - 1].rarity);
        TryGetUsableBaitForFish(playerInventory, areaType, lastFishRarity, out selectedBait);

        return validCandidates[validCandidates.Count - 1];
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
            case "전설":
            case "마스터":
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
                return "아이템 등급이 낮습니다.";

            case EFishingStartResult.NoCatchableFishForBait:
                return "미끼의 등급이 낮습니다.";

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

    private void OnDisable()
    {
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
