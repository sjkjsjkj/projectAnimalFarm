using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 채집 가능한 오브젝트.
/// 
/// 이 스크립트의 역할
/// 1. 팀 상호작용 시스템에서 호출할 수 있도록 IInteractable을 구현한다.
/// 2. 플레이어가 상호작용하면 실제 채집을 진행한다.
/// 3. 보상 아이템은 플레이어의 CPlayerCollector2D를 통해 지급한다.
/// 4. 필요하면 자동 채집도 지원한다.
/// 5. 획득 후 비활성화 / 파괴 / 리스폰을 처리할 수 있다.
/// 
/// 사용 예시
/// - 약초
/// - 광석
/// - 나무
/// - 열매 오브젝트
/// - 채집 가능한 풀
/// 
/// 중요
/// - 이 오브젝트는 Interact 레이어를 사용
/// - Collider2D 필요
/// - 팀 상호작용 시스템이 Interact(GameObject player)를 호출
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CCollectableInteractObject2D : BaseMono, IInteractable
{
    public enum ECollectMode
    {
        AutoOnly = 0,     // 자동 채집만 가능
        ButtonOnly,       // 버튼 상호작용으로만 채집 가능
        AutoOrButton,     // 자동 / 버튼 둘 다 가능
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

    [Header("수동 채집 연출")]
    [Tooltip("버튼 채집 시 채집 애니메이션 / 딜레이를 사용할지")]
    [SerializeField] private bool _useInteractionMotion = true;

    [Tooltip("버튼 채집 시 실제 보상 지급까지 대기 시간")]
    [SerializeField] private float _manualCollectDelay = 1.0f;

    [Tooltip("버튼 채집 중 플레이어를 Busy 상태로 묶을지")]
    [SerializeField] private bool _setPlayerBusyDuringManualCollect = true;

    [Tooltip("채집 성공 시 지급할 생활 스킬 경험치")]
    [SerializeField] private int _gatherSkillExp = 0;

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

    [Header("로그")]
    [SerializeField] private bool _logEnabled = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    /// <summary>
    /// 현재 범위 안에 들어와 있는 플레이어 수집기 목록
    /// 자동 채집에 사용
    /// </summary>
    private readonly List<CPlayerCollector2D> _nearCollectors = new List<CPlayerCollector2D>();

    /// <summary>
    /// 이미 획득된 상태인지
    /// </summary>
    private bool _isCollected = false;

    /// <summary>
    /// 현재 처리 중인지
    /// 중복 채집 방지용
    /// </summary>
    private bool _isProcessing = false;

    private Coroutine _autoCollectRoutine = null;
    private Coroutine _respawnRoutine = null;
    private Collider2D _triggerCollider = null;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public bool IsCollectableNow => !_isCollected && !_isProcessing;
    public bool CanManualCollect => _collectMode == ECollectMode.ButtonOnly || _collectMode == ECollectMode.AutoOrButton;
    public bool CanAutoCollect => _collectMode == ECollectMode.AutoOnly || _collectMode == ECollectMode.AutoOrButton;
    #endregion

    #region ─────────────────────────▶ IInteractable 구현 ◀─────────────────────────
    /// <summary>
    /// 팀 상호작용 시스템에서 먼저 호출하는 상호작용 가능 여부 검사
    /// </summary>
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

        return CanCollect(collector);
    }

    /// <summary>
    /// 팀 상호작용 시스템에서 실제 상호작용 시 호출
    /// </summary>
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

    /// <summary>
    /// 팀 상호작용 UI에 보여줄 문구
    /// </summary>
    public string GetMessage()
    {
        string targetName = string.IsNullOrWhiteSpace(_displayName) ? _itemId : _displayName;
        return $"{targetName} 채집";
    }
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

        return true;
    }

    /// <summary>
    /// 수동 채집 시도
    /// 팀 상호작용 시스템에서 최종적으로 여기까지 오게 된다.
    /// </summary>
    public bool TryCollect(CPlayerCollector2D collector)
    {
        return TryCollectInternal(collector, true);
    }

    /// <summary>
    /// 네가 별도 UI 문구를 띄우고 싶을 때 사용할 수 있는 보조 함수
    /// </summary>
    public string GetInteractionMessage(KeyCode key)
    {
        string targetName = string.IsNullOrWhiteSpace(_displayName) ? _itemId : _displayName;
        return $"[{key}] {targetName} 채집";
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 실제 채집 공용 처리
    /// isManualRequest가 true면 버튼 채집
    /// false면 자동 채집
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

        // 수동 채집이고 모션을 쓰는 경우
        // 즉시 지급하지 않고 코루틴으로 연출 후 지급
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
    /// 수동 채집용 코루틴
    /// - 시작 이벤트
    /// - 플레이어 Busy
    /// - 애니메이션
    /// - 대기
    /// - 보상 지급
    /// - 경험치 지급
    /// - 완료 처리
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
    /// 실제 아이템 지급 처리
    /// 
    /// 중요
    /// - 여기서 직접 인벤토리를 건드리지 않음
    /// - 플레이어의 CPlayerCollector2D를 통해
    ///   ItemCollectionCoordinator로 넘겨서 처리
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
    /// 자동 채집 시작
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
    /// 자동 채집 중지
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
    /// 자동 채집 코루틴
    /// 범위 안 플레이어가 있으면 잠깐 기다렸다가 지급 시도
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
    /// 획득 완료 처리
    /// - 상태 변경
    /// - 범위 목록 정리
    /// - 콜라이더 끄기
    /// - 이벤트 호출
    /// - 파괴 / 비활성화 / 리스폰 처리
    /// </summary>
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

    /// <summary>
    /// 리스폰 대기 후 복구
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
    /// 리스폰 시 상태 초기화
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
    /// 획득 후 비주얼 On/Off 처리
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
    /// null collector 정리
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
    /// 범위 안에 collector가 하나라도 있는지
    /// </summary>
    private bool HasAnyCollectorInRange()
    {
        CleanupCollectors();
        return _nearCollectors.Count > 0;
    }

    /// <summary>
    /// 현재 콜라이더 범위를 다시 검사해서
    /// 근처 플레이어 collector 목록 복구
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
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        _triggerCollider = GetComponent<Collider2D>();

        // 트리거가 아니면 자동 감지가 안 될 수 있으니 경고 출력
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
        StopAutoCollect();

        if (_respawnRoutine != null)
        {
            StopCoroutine(_respawnRoutine);
            _respawnRoutine = null;
        }
    }
    #endregion
}
