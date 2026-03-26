using UnityEngine;

/// <summary>
/// 2D 상호작용 오브젝트의 공통 규칙을 관리하는 베이스 클래스
/// </summary>
public abstract class CInteractableBase2D : MonoBehaviour
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("텍스트")]
    [SerializeField] private string _displayName = "상호작용 대상";
    [SerializeField] private string _verbText = "사용";

    [Header("공통 규칙")]
    [SerializeField] private bool _useOnce = false;
    [SerializeField] private float _cooldown = 0f;

    [Header("힌트")]
    [SerializeField] private Transform _hintAnchor;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _usedOnce = false;
    private float _nextInteractTime = 0f;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public string DisplayName => _displayName;
    public string VerbText => _verbText;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    protected virtual bool CanInteract(CInteractor2D interactor, out string failReason)
    {
        failReason = string.Empty;

        if (interactor == null)
        {
            failReason = "상호작용 주체가 없습니다.";
            return false;
        }

        if (!isActiveAndEnabled)
        {
            failReason = "현재 비활성화 상태입니다.";
            return false;
        }

        return true;
    }

    protected abstract bool OnInteract(CInteractor2D interactor);

    private void ApplyCommonAfterInteract()
    {
        if (_cooldown > 0f)
        {
            _nextInteractTime = Time.time + _cooldown;
        }

        if (_useOnce)
        {
            _usedOnce = true;
        }
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public bool IsAvailable()
    {
        if (_usedOnce)
        {
            return false;
        }

        if (Time.time < _nextInteractTime)
        {
            return false;
        }

        return true;
    }

    public Vector3 GetHintAnchorPosition()
    {
        if (_hintAnchor != null)
        {
            return _hintAnchor.position;
        }

        return transform.position;
    }

    public bool TryInteract(CInteractor2D interactor)
    {
        if (!IsAvailable())
        {
            Debug.Log($"[{name}] 현재 사용할 수 없습니다. (일회성 또는 쿨타임)");
            return false;
        }

        if (!CanInteract(interactor, out string failReason))
        {
            if (failReason.HasValue())
            {
                Debug.Log($"[{name}] 상호작용 실패: {failReason}");
            }

            return false;
        }

        bool isSuccess = OnInteract(interactor);
        if (!isSuccess)
        {
            return false;
        }

        ApplyCommonAfterInteract();
        return true;
    }
    #endregion
}
