using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 채집 가능한 오브젝트
/// - 플레이어가 가까이 가면 자동 습득 가능
/// - 버튼을 눌러서 습득 가능
/// - 버튼 채집 시에는 모션 / 딜레이를 줄 수 있음
/// - 획득 후 비활성화 / 파괴 / 리스폰 처리 가능
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CCollectableInteractObject2D : BaseMono, IInteractable
{
    public enum ECollectMode
    {
        AutoOnly = 0,     // 범위 안에 오면 자동 습득
        ButtonOnly,       // 버튼 입력으로만 습득
        AutoOrButton,     // 자동 습득도 가능, 버튼으로도 가능
    }

    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("지급 아이템 정보")]
    [SerializeField] private string _itemId;
    [SerializeField] private int _amount = 1;
    [SerializeField] private string _displayName = "";

    [Header("획득 방식")]
    [SerializeField] private ECollectMode _collectMode = ECollectMode.ButtonOnly;

    [Tooltip("AutoOnly / AutoOrButton에서 자동 습득까지 걸리는 시간")]
    [SerializeField] private float _autoCollectDelay = 0.15f;

    [Header("수동 채집 연출")]
    [Tooltip("버튼 채집 시 플레이어 모션 / 딜레이를 사용할지")]
    [SerializeField] private bool _useInteractionMotion = true;

    [Tooltip("E키를 눌렀을 때 실제 보상을 주기까지 대기 시간")]
    [SerializeField] private float _manualCollectDelay = 1.0f;

    [Tooltip("버튼 채집 중 플레이어를 Busy 상태로 묶을지")]
    [SerializeField] private bool _setPlayerBusyDuringManualCollect = true;

    [Tooltip("채집 성공 시 지급할 채집 경험치")]
    [SerializeField] private int _gatherSkillExp = 0;

    [Header("획득 후 처리")]
    [SerializeField] private bool _destroyOnCollected = false;
    [SerializeField] private GameObject _disableTargetAfterCollected;

    [Header("리스폰 설정")]
    [SerializeField] private bool _useRespawn = false;
    [SerializeField] private float _respawnDelay = 10f;

    [Header("이벤트")]
    [SerializeField] private UnityEvent _onCollectStarted;
    [SerializeField] private UnityEvent _onCollected;
    [SerializeField] private UnityEvent _onRespawned;

    [Header("로그 출력")]
    [SerializeField] private bool _logEnabled = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    // 범위 안에 들어와 있는 플레이어 수집기 목록
    private readonly List<CPlayerCollector2D> _nearCollectors = new List<CPlayerCollector2D>();

    // 이미 획득되어 현재 사용할 수 없는 상태인지 여부
    private bool _isCollected = false;

    // 현재 처리 중인지 여부
    // 수동 채집 코루틴이 도는 동안 true
    private bool _isProcessing = false;

    // 자동 습득 코루틴 핸들
    private Coroutine _autoCollectRoutine = null;

    // 리스폰 코루틴 핸들
    private Coroutine _respawnRoutine = null;

    // 트리거 콜라이더 캐시
    private Collider2D _triggerCollider = null;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public bool IsCollectableNow => !_isCollected && !_isProcessing;
    public bool CanManualCollect => _collectMode == ECollectMode.ButtonOnly || _collectMode == ECollectMode.AutoOrButton;
    public bool CanAutoCollect => _collectMode == ECollectMode.AutoOnly || _collectMode == ECollectMode.AutoOrButton;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 현재 이 오브젝트가 해당 플레이어 기준으로 채집 가능한지 검사
    /// </summary>
    public bool CanCollect(CPlayerCollector2D collector)
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

        if (string.IsNullOrWhiteSpace(_itemId))
        {
            return false;
        }

        if (_amount <= 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 플레이어가 버튼 채집 시도할 때 호출
    /// </summary>
    public bool TryCollect(CPlayerCollector2D collector)
    {
        return TryCollectInternal(collector, true);
    }

    /// <summary>
    /// 안내 문구 생성
    /// </summary>
    public string GetInteractionMessage(KeyCode key)
    {
        string targetName = string.IsNullOrWhiteSpace(_displayName) ? _itemId : _displayName;
        return $"[{key}] {targetName} 채집";
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 실제 채집 시도 공통 처리
    /// isManualRequest == true 이면 E키로 눌러서 채집한 경우
    /// </summary>
    private bool TryCollectInternal(CPlayerCollector2D collector, bool isManualRequest)
    {
        if (!CanCollect(collector))
        {
            return false;
        }

        if (isManualRequest && !CanManualCollect)
        {
            return false;
        }

        if (!isManualRequest && !CanAutoCollect)
        {
            return false;
        }

        // 버튼 채집인 경우
        // 플레이어 모션을 쓰는 설정이면 코루틴으로 들어가서 딜레이 후 지급
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

            StartCoroutine(CoManualCollect(collector));
            return true;
        }

        // 자동 채집 또는 모션 없는 즉시 채집
        bool received = TryGiveReward(collector);

        if (!received)
        {
            return false;
        }

        if (_gatherSkillExp > 0)
        {
            collector.TryAddLifeSkillExp(ELifeSkill.Gathering, _gatherSkillExp);
        }

        CompleteCollect();
        return true;
    }

    /// <summary>
    /// 버튼 채집 실제 처리
    /// - 모션 재생
    /// - 딜레이 대기
    /// - 아이템 지급
    /// - 성공 시 완료 처리
    /// </summary>
    private IEnumerator CoManualCollect(CPlayerCollector2D collector)
    {
        if (_isProcessing)
        {
            yield break;
        }

        _isProcessing = true;
        _onCollectStarted?.Invoke();

        if (_setPlayerBusyDuringManualCollect && collector != null)
        {
            collector.SetInteractionBusy(true);
            collector.PlayGatherAnimation();
        }

        if (_manualCollectDelay > 0f)
        {
            yield return new WaitForSeconds(_manualCollectDelay);
        }

        bool received = TryGiveReward(collector);

        if (received && collector != null && _gatherSkillExp > 0)
        {
            collector.TryAddLifeSkillExp(ELifeSkill.Gathering, _gatherSkillExp);
        }

        if (received)
        {
            CompleteCollect();
        }
        else if (_logEnabled)
        {
            Debug.LogWarning($"[CCollectableInteractObject2D] 수동 채집 실패. itemId={_itemId}, object={name}");
        }

        _isProcessing = false;

        if (_setPlayerBusyDuringManualCollect && collector != null)
        {
            collector.SetInteractionBusy(false);
        }
    }

    /// <summary>
    /// 실제 아이템 지급
    /// 성공하면 true, 실패하면 false
    /// </summary>
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

        // 플레이어에게 실제 아이템 지급
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

    /// <summary>
    /// 자동 습득 코루틴 시작
    /// </summary>
    private void StartAutoCollect()
    {
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

    /// <summary>
    /// 자동 습득 중지
    /// </summary>
    private void StopAutoCollect()
    {
        if (_autoCollectRoutine == null)
        {
            return;
        }

        StopCoroutine(_autoCollectRoutine);
        _autoCollectRoutine = null;
    }

    /// <summary>
    /// 자동 습득 실제 처리
    /// 자동 습득은 버튼 모션 없이 바로 지급되도록 유지
    /// </summary>
    private IEnumerator CoAutoCollect()
    {
        if (_autoCollectDelay > 0f)
        {
            yield return new WaitForSeconds(_autoCollectDelay);
        }

        _autoCollectRoutine = null;

        if (_isCollected || _isProcessing)
        {
            yield break;
        }

        // 범위 안에 남아있는 플레이어 중 첫 번째 유효한 플레이어에게 지급
        for (int i = 0; i < _nearCollectors.Count; i++)
        {
            CPlayerCollector2D collector = _nearCollectors[i];

            if (collector == null)
            {
                continue;
            }

            if (TryCollectInternal(collector, false))
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// 실제 획득 완료 처리
    /// - 플레이어 목록 정리
    /// - 콜라이더 비활성화
    /// - 이벤트 호출
    /// - 비활성화 / 파괴 / 리스폰 분기
    /// </summary>
    private void CompleteCollect()
    {
        if (_isCollected)
        {
            return;
        }

        _isCollected = true;

        // 범위에 등록된 플레이어들에게서 자신을 제거
        for (int i = 0; i < _nearCollectors.Count; i++)
        {
            CPlayerCollector2D collector = _nearCollectors[i];

            if (collector == null)
            {
                continue;
            }

            collector.UnregisterNearbyTarget(this);
        }

        _nearCollectors.Clear();

        StopAutoCollect();

        // 트리거 비활성화
        if (_triggerCollider != null)
        {
            _triggerCollider.enabled = false;
        }

        if (_logEnabled)
        {
            Debug.Log($"[CCollectableInteractObject2D] 획득 완료. itemId={_itemId}, amount={_amount}, object={name}");
        }

        _onCollected?.Invoke();

        // 리스폰 모드가 켜져 있으면 자기 자신은 살아 있어야 코루틴이 돈다.
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

        // 리스폰이 아니라면 기존 로직대로 파괴 / 비활성화
        if (_destroyOnCollected)
        {
            Destroy(gameObject);
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

    /// <summary>
    /// 리스폰 코루틴
    /// 일정 시간 후 다시 채집 가능 상태로 복구
    /// </summary>
    private IEnumerator CoRespawn()
    {
        if (_respawnDelay > 0f)
        {
            yield return new WaitForSeconds(_respawnDelay);
        }

        _respawnRoutine = null;
        ResetCollectState();
    }

    /// <summary>
    /// 리스폰 후 상태 초기화
    /// </summary>
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

    /// <summary>
    /// 획득 후 비주얼 On/Off
    /// 리스폰 구조에서는 자기 자신을 꺼버리면 코루틴이 멈추므로
    /// 가능하면 _disableTargetAfterCollected 에 비주얼 루트를 연결해서 쓰는 것이 좋다.
    /// </summary>
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

    /// <summary>
    /// 플레이어 수집기 목록 정리
    /// </summary>
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

    /// <summary>
    /// 현재 범위 안에 플레이어가 있는지 여부
    /// </summary>
    private bool HasAnyCollectorInRange()
    {
        CleanupCollectors();
        return _nearCollectors.Count > 0;
    }

    /// <summary>
    /// 리스폰 직후 플레이어가 이미 범위 안에 서 있는 경우
    /// OnTriggerEnter2D가 다시 안 들어올 수 있으므로 수동으로 재등록
    /// </summary>
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
            collector.RegisterNearbyTarget(this);
        }
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

        collector.RegisterNearbyTarget(this);

        // 자동 습득 가능 모드면 자동 채집 시작
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
        collector.UnregisterNearbyTarget(this);

        // 범위 안 플레이어가 하나도 없으면 자동 습득 중단
        if (!HasAnyCollectorInRange())
        {
            StopAutoCollect();
        }
    }

    private void OnDisable()
    {
        StopAutoCollect();

        if (_respawnRoutine != null)
        {
            StopCoroutine(_respawnRoutine);
            _respawnRoutine = null;
        }
    }

    public bool Interact(GameObject player) {

        return false;
    }
    #endregion
}
