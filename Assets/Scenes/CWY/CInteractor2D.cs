using UnityEngine;

/// <summary>
/// 2D 플레이어 상호작용 탐지 및 입력 처리
/// </summary>
public class CInteractor2D : MonoBehaviour
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("탐지 기준")]
    [SerializeField] private Transform _origin;
    [SerializeField] private float _radius = 1.5f;
    [SerializeField] private LayerMask _interactableLayer;

    [Header("입력")]
    [SerializeField] private KeyCode _interactKey = KeyCode.E;

    [Header("디버그")]
    [SerializeField] private bool _logHint = true;
    [SerializeField] private bool _showGizmo = true;
    [SerializeField] private int _bufferSize = 16;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private CInteractableBase2D _current;
    private Collider2D[] _hitBuffer;
    private float _nextHintLogTime = 0f;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public CInteractableBase2D Current => _current;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void EnsureBuffer()
    {
        if (_bufferSize < 1)
        {
            _bufferSize = 1;
        }

        if (_hitBuffer == null || _hitBuffer.Length != _bufferSize)
        {
            _hitBuffer = new Collider2D[_bufferSize];
        }
    }

    private Vector3 GetOriginPosition()
    {
        if (_origin != null)
        {
            return _origin.position;
        }

        return transform.position;
    }

    private void FindBestInteractable()
    {
        EnsureBuffer();

        Vector3 originPosition = GetOriginPosition();
        int hitCount = Physics2D.OverlapCircleNonAlloc(originPosition, _radius, _hitBuffer, _interactableLayer);

        if (hitCount <= 0)
        {
            _current = null;
            return;
        }

        CInteractableBase2D best = null;
        float bestDistanceSqr = float.MaxValue;

        for (int i = 0; i < hitCount; ++i)
        {
            Collider2D hit = _hitBuffer[i];
            if (hit == null)
            {
                continue;
            }

            CInteractableBase2D target = hit.GetComponentInParent<CInteractableBase2D>();
            if (target == null)
            {
                continue;
            }

            if (!target.IsAvailable())
            {
                continue;
            }

            Vector3 targetPosition = target.GetHintAnchorPosition();
            float distanceSqr = (targetPosition - originPosition).sqrMagnitude;

            if (distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                best = target;
            }
        }

        _current = best;
    }

    private void PrintHintIfNeeded()
    {
        if (!_logHint)
        {
            return;
        }

        if (Time.time < _nextHintLogTime)
        {
            return;
        }

        _nextHintLogTime = Time.time + 0.35f;

        if (_current == null)
        {
            return;
        }

        Debug.Log($"[{_interactKey}] {_current.DisplayName} {_current.VerbText}");
    }

    private void TryInteract()
    {
        if (_current == null)
        {
            return;
        }

        _current.TryInteract(this);
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public bool TryGetItemReceiver(out IItemReceiver itemReceiver)
    {
        itemReceiver = GetComponent<IItemReceiver>();
        return itemReceiver != null;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        EnsureBuffer();
    }

    private void Update()
    {
        FindBestInteractable();
        PrintHintIfNeeded();

        if (Input.GetKeyDown(_interactKey))
        {
            TryInteract();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!_showGizmo)
        {
            return;
        }

        Vector3 originPosition = (_origin != null) ? _origin.position : transform.position;

        Gizmos.color = new Color(0.2f, 1f, 0.8f, 0.35f);
        Gizmos.DrawSphere(originPosition, _radius);
    }
    #endregion
}
