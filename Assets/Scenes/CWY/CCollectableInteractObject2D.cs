using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 채집 가능한 오브젝트
/// - 플레이어가 가까이 가면 자동 습득 가능
/// - 버튼을 눌러서 습득 가능
/// - 두 방식 모두 허용 가능
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CCollectableInteractObject2D : MonoBehaviour
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

    [Header("획득 후 처리")]
    [SerializeField] private bool _destroyOnCollected = false;
    [SerializeField] private GameObject _disableTargetAfterCollected;

    [Header("이벤트")]
    [SerializeField] private UnityEvent _onCollected;

    [Header("로그 출력")]
    [SerializeField] private bool _logEnabled = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    // 범위 안에 들어와 있는 플레이어 수집기 목록
    private readonly List<CPlayerCollector2D> _nearCollectors = new List<CPlayerCollector2D>();

    // 이미 획득되었는지 여부
    private bool _isCollected = false;

    // 자동 습득 코루틴 핸들
    private Coroutine _autoCollectRoutine = null;

    // 트리거 콜라이더 캐시
    private Collider2D _triggerCollider = null;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public bool IsCollectableNow => !_isCollected;
    public bool CanManualCollect => _collectMode == ECollectMode.ButtonOnly || _collectMode == ECollectMode.AutoOrButton;
    public bool CanAutoCollect => _collectMode == ECollectMode.AutoOnly || _collectMode == ECollectMode.AutoOrButton;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 플레이어가 수동/자동 방식으로 이 오브젝트를 획득 시도할 때 호출
    /// </summary>
    public bool TryCollect(CPlayerCollector2D collector)
    {
        if (_isCollected)
        {
            return false;
        }

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

        CompleteCollect();
        return true;
    }

    /// <summary>
    /// 안내 문구 생성
    /// </summary>
    public string GetInteractionMessage(KeyCode key)
    {
        string targetName = string.IsNullOrWhiteSpace(_displayName) ? _itemId : _displayName;
        return $"[{key}] {targetName} 획득";
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
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
    /// </summary>
    private IEnumerator CoAutoCollect()
    {
        if (_autoCollectDelay > 0f)
        {
            yield return new WaitForSeconds(_autoCollectDelay);
        }

        _autoCollectRoutine = null;

        if (_isCollected)
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

            if (TryCollect(collector))
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// 실제 획득 완료 처리
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

        // 획득 후 처리
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
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
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
    }
    #endregion
}
