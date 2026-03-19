using System.Collections;
using UnityEngine;

/// <summary>
/// 2D 채집 노드
/// 예: 약초, 나무, 돌
/// </summary>
public class CGatherNode2D : CInteractableBase2D
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("드랍 설정")]
    [SerializeField] private EItem _rewardItem = EItem.Herb;
    [SerializeField] private int _rewardAmount = 1;

    [Header("리스폰 설정")]
    [SerializeField] private bool _useRespawn = false;
    [SerializeField] private float _respawnTime = 5f;

    [Header("비주얼 / 충돌")]
    [SerializeField] private GameObject _visualRoot;
    [SerializeField] private Collider2D _targetCollider;

    [Header("디버그")]
    [SerializeField] private bool _logOnGather = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isDepleted = false;
    private Coroutine _respawnCoroutine;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    protected override bool CanInteract(CInteractor2D interactor, out string failReason)
    {
        if (!base.CanInteract(interactor, out failReason))
        {
            return false;
        }

        if (_isDepleted)
        {
            failReason = "이미 채집된 상태입니다.";
            return false;
        }

        if (_rewardItem == EItem.None)
        {
            failReason = "보상 아이템이 설정되지 않았습니다.";
            return false;
        }

        if (_rewardAmount <= 0)
        {
            failReason = "보상 수량이 올바르지 않습니다.";
            return false;
        }

        if (!interactor.TryGetItemReceiver(out IItemReceiver itemReceiver))
        {
            failReason = "아이템 수령 컴포넌트가 없습니다.";
            return false;
        }

        if (itemReceiver == null)
        {
            failReason = "아이템 수령 대상이 비어 있습니다.";
            return false;
        }

        return true;
    }

    protected override bool OnInteract(CInteractor2D interactor)
    {
        if (interactor == null)
        {
            return false;
        }

        if (!interactor.TryGetItemReceiver(out IItemReceiver itemReceiver))
        {
            return false;
        }

        bool isAdded = itemReceiver.TryAddItem(_rewardItem, _rewardAmount);
        if (!isAdded)
        {
            Debug.LogWarning($"[{name}] 아이템 지급에 실패했습니다.");
            return false;
        }

        if (_logOnGather)
        {
            Debug.Log($"[채집 성공] {_rewardItem} x{_rewardAmount}");
        }

        SetDepleted(true);

        if (_useRespawn)
        {
            StartRespawnRoutine();
        }

        return true;
    }

    private void SetDepleted(bool isDepleted)
    {
        _isDepleted = isDepleted;

        if (_visualRoot != null)
        {
            _visualRoot.SetActive(!isDepleted);
        }

        if (_targetCollider != null)
        {
            _targetCollider.enabled = !isDepleted;
        }
    }

    private void StartRespawnRoutine()
    {
        if (_respawnTime <= 0f)
        {
            Respawn();
            return;
        }

        if (_respawnCoroutine != null)
        {
            StopCoroutine(_respawnCoroutine);
        }

        _respawnCoroutine = StartCoroutine(CoRespawn());
    }

    private IEnumerator CoRespawn()
    {
        yield return new WaitForSeconds(_respawnTime);

        Respawn();
        _respawnCoroutine = null;
    }

    private void Respawn()
    {
        SetDepleted(false);

        if (_logOnGather)
        {
            Debug.Log($"[채집 리스폰] {name}");
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Reset()
    {
        if (_targetCollider == null)
        {
            _targetCollider = GetComponent<Collider2D>();
        }

        if (_visualRoot == null)
        {
            _visualRoot = gameObject;
        }
    }

    private void OnDisable()
    {
        if (_respawnCoroutine != null)
        {
            StopCoroutine(_respawnCoroutine);
            _respawnCoroutine = null;
        }
    }
    #endregion
}
