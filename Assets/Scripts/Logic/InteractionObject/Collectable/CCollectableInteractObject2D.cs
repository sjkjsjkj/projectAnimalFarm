using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 채집 가능한 오브젝트
///
/// 이번 수정 핵심
/// 1. 버튼 채집은 "가까운 거리" 안에서만 가능
/// 2. ButtonOnly일 때 자동 채집이 절대 발생하지 않도록 보강
/// 3. AutoOnly / AutoOrButton도 자동 채집 거리 제한 추가
/// 4. 수동 채집 도중 이동 시 취소
/// 5. 피드백 UI / 사운드는 나중에 Inspector 이벤트로 연결 가능
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CCollectableInteractObject2D : BaseMono, IInteractable
{
    [Serializable]
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

    [Header("수동 채집 연출")]
    [Tooltip("버튼 채집 시 채집 애니메이션 / 딜레이를 사용할지")]
    [SerializeField] private bool _useInteractionMotion = true;

    [Tooltip("버튼 채집 시 실제 보상 지급까지 대기 시간")]
    [SerializeField] private float _manualCollectDelay = 1.0f;

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
    [Tooltip("피드백 UI를 나중에 연결할 때 사용. string 메시지를 받는 함수 연결")]
    [SerializeField] private StringEvent _onFeedbackMessage;

    [Tooltip("채집 실패 시 호출. 나중에 사운드/VFX 연결 가능")]
    [SerializeField] private UnityEvent _onCollectFailed;

    [Tooltip("채집 취소 시 호출. 나중에 사운드/VFX 연결 가능")]
    [SerializeField] private UnityEvent _onCollectCanceled;

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
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
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

        return CanCollect(collector, true);
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

        TryCollect(collector);
    }

    public string GetMessage()
    {
        string targetName = string.IsNullOrWhiteSpace(_displayName) ? _itemId : _displayName;
        return $"{targetName} 채집";
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public bool CanCollect(CPlayerCollector2D collector)
    {
        return CanCollect(collector, true);
    }

    public bool TryCollect(CPlayerCollector2D collector)
    {
        return TryCollectInternal(collector, true);
    }

    public string GetInteractionMessage(KeyCode key)
    {
        string targetName = string.IsNullOrWhiteSpace(_displayName) ? _itemId : _displayName;
        return $"[{key}] {targetName} 채집";
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 수동/자동 여부에 따라 거리 제한을 다르게 적용
    /// </summary>
    private bool CanCollect(CPlayerCollector2D collector, bool isManual)
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

    private bool TryCollectInternal(CPlayerCollector2D collector, bool isManualRequest)
    {
        if (collector == null)
        {
            return false;
        }

        // 버튼 채집은 버튼 가능한 모드에서만
        if (isManualRequest && !CanManualCollect)
        {
            return false;
        }

        // 자동 채집은 자동 가능한 모드에서만
        if (!isManualRequest && !CanAutoCollect)
        {
            return false;
        }

        if (!CanCollect(collector, isManualRequest))
        {
            if (_logEnabled)
            {
                Debug.Log($"[CCollectableInteractObject2D] 거리 또는 상태 조건 미충족. object={name}, manual={isManualRequest}");
            }

            return false;
        }

        // 수동 채집 + 모션 사용이면 딜레이 동안 이동 취소 가능하게 코루틴 처리
        if (isManualRequest && _useInteractionMotion)
        {
            if (!collector.CanStartInteraction())
            {
                if (_logEnabled)
                {
                    Debug.Log($"[CCollectableInteractObject2D] 플레이어가 Busy 상태라 채집 시작 불가. object={name}");
                }

                return false;
            }

            StartManualCollectRoutine(collector);
            return true;
        }

        // 자동 채집 또는 즉시 채집
        bool received = TryGiveReward(collector);

        if (!received)
        {
            ShowReservedFeedback("아이템을 획득하지 못했습니다.");
            _onCollectFailed?.Invoke();
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
            return;
        }

        _manualCollectCollector = collector;
        _manualCollectStartPosition = collector != null ? (Vector2)collector.transform.position : Vector2.zero;

        SubscribeMoveCancel();
        _onCollectStarted?.Invoke();

        _manualCollectRoutine = StartCoroutine(CoManualCollect(collector));
    }

    private IEnumerator CoManualCollect(CPlayerCollector2D collector)
    {
        if (_isProcessing)
        {
            yield break;
        }

        _isProcessing = true;

        if (_setPlayerBusyDuringManualCollect && collector != null)
        {
            collector.SetInteractionBusy(true);
            collector.PlayGatherAnimation();
        }

        if (_manualCollectDelay > 0f)
        {
            yield return new WaitForSeconds(_manualCollectDelay);
        }

        // 이동 이벤트를 놓친 경우를 대비한 마지막 안전 검사
        if (HasCollectorMovedSinceStart(collector))
        {
            CancelManualCollectInternal(_manualCollectCancelMessage, true);
            yield break;
        }

        // 딜레이가 끝난 시점에도 가까운 거리 유지 중인지 다시 검사
        if (!IsCollectorWithinDistance(collector, _manualCollectMaxDistance))
        {
            CancelManualCollectInternal("채집 범위를 벗어나 채집이 취소되었습니다.", true);
            yield break;
        }

        bool received = TryGiveReward(collector);

        // 보상 처리 전에 상태부터 풀어줘야 비활성화/파괴돼도 Busy가 남지 않음
        ReleaseManualCollectState(collector);

        if (!received)
        {
            if (_logEnabled)
            {
                Debug.LogWarning($"[CCollectableInteractObject2D] 수동 채집 실패. itemId={_itemId}, object={name}");
            }

            ShowReservedFeedback("아이템을 획득하지 못했습니다.");
            _onCollectFailed?.Invoke();
            yield break;
        }

        if (collector != null && _gatherSkillExp > 0)
        {
            collector.TryAddLifeSkillExp(ELifeSkill.Gathering, _gatherSkillExp);
        }

        CompleteCollect();
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

        if (showFeedback)
        {
            ShowReservedFeedback(message);
        }

        _onCollectCanceled?.Invoke();
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

        CancelManualCollectInternal(_manualCollectCancelMessage, true);
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

    /// <summary>
    /// 콜라이더 기준 실제 가까운 거리 계산
    /// 오브젝트 중심이 아니라 가장 가까운 콜라이더 표면 기준이라 더 정확함
    /// </summary>
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

        return true;
    }

    private void StartAutoCollect()
    {
        // ButtonOnly면 자동 채집 절대 금지
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
            yield return new WaitForSeconds(_autoCollectDelay);
        }

        _autoCollectRoutine = null;

        // ButtonOnly면 자동 채집 절대 금지
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

            // 자동 채집도 거리 제한을 다시 걸어준다
            if (!IsCollectorWithinDistance(collector, _autoCollectMaxDistance))
            {
                continue;
            }

            if (TryCollectInternal(collector, false))
            {
                yield break;
            }
        }
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

        if (_triggerCollider != null)
        {
            _triggerCollider.enabled = false;
        }

        if (_logEnabled)
        {
            Debug.Log($"[CCollectableInteractObject2D] 획득 완료. itemId={_itemId}, amount={_amount}, object={name}");
        }

        _onCollected?.Invoke();

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

        _onFeedbackMessage?.Invoke(message);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        _triggerCollider = GetComponent<Collider2D>();

        if (_triggerCollider != null && !_triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[CCollectableInteractObject2D] {name}의 Collider2D가 Trigger가 아닙니다. 자동/버튼 채집 범위 감지가 안 될 수 있습니다.");
        }
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

        // ButtonOnly면 자동 채집 시작 금지
        if (CanAutoCollect)
        {
            StartAutoCollect();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        CPlayerCollector2D collector = other.GetComponentInParent<CPlayerCollector2D>();

        if (collector == null)
        {
            return;
        }

        _nearCollectors.Remove(collector);

        if (!HasAnyCollectorInRange())
        {
            StopAutoCollect();
        }
    }

    private void OnDisable()
    {
        CancelManualCollectInternal(string.Empty, false);

        StopAutoCollect();

        if (_respawnRoutine != null)
        {
            StopCoroutine(_respawnRoutine);
            _respawnRoutine = null;
        }
    }
    #endregion
}
