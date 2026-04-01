using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 플레이어가 주변 채집 오브젝트와 상호작용할 수 있게 해주는 스크립트
/// - 버튼 입력(E키 등)으로 수동 획득
/// - ItemCollectionCoordinator를 통해 인벤토리/도감에 반영
/// </summary>
public class CPlayerCollector2D : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("입력 설정")]
    [SerializeField] private KeyCode _collectKey = KeyCode.E;

    [Header("아이템 지급 관리자")]
    [SerializeField] private ItemCollectionCoordinator _collectionCoordinator;

    [Header("인터렉션 안내 UI (선택)")]
    [SerializeField] private TMP_Text _interactionText;

    [Header("로그 출력")]
    [SerializeField] private bool _logEnabled = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    // 현재 플레이어 근처에 들어와 있는 채집 가능한 오브젝트 목록
    private readonly List<CCollectableInteractObject2D> _nearTargets = new List<CCollectableInteractObject2D>();
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public KeyCode CollectKey => _collectKey;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 채집 오브젝트가 플레이어 근처에 들어왔을 때 등록
    /// </summary>
    public void RegisterNearbyTarget(CCollectableInteractObject2D target)
    {
        if (target == null)
        {
            return;
        }

        if (_nearTargets.Contains(target))
        {
            return;
        }

        _nearTargets.Add(target);

        if (_logEnabled)
        {
            Debug.Log($"[CPlayerCollector2D] 타겟 등록: {target.name}");
        }
    }

    /// <summary>
    /// 채집 오브젝트가 플레이어 범위 밖으로 나갔을 때 해제
    /// </summary>
    public void UnregisterNearbyTarget(CCollectableInteractObject2D target)
    {
        if (target == null)
        {
            return;
        }

        _nearTargets.Remove(target);

        if (_logEnabled)
        {
            Debug.Log($"[CPlayerCollector2D] 타겟 해제: {target.name}");
        }
    }

    /// <summary>
    /// 실제 아이템을 지급받는 함수
    /// 채집 오브젝트가 이 함수를 호출해서 아이템을 플레이어에게 준다.
    /// </summary>
    public bool TryReceiveItem(string itemId, int amount)
    {
        if (_collectionCoordinator == null)
        {
            Debug.LogError("[CPlayerCollector2D] ItemCollectionCoordinator가 연결되지 않았습니다.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogWarning("[CPlayerCollector2D] itemId가 비어 있습니다.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"[CPlayerCollector2D] amount가 0 이하입니다. itemId={itemId}, amount={amount}");
            return false;
        }

        return _collectionCoordinator.TryCollectItem(itemId, amount);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// null이 된 타겟 제거
    /// 파괴된 오브젝트가 리스트에 남아 있을 수 있으므로 정리
    /// </summary>
    private void CleanupNullTargets()
    {
        for (int i = _nearTargets.Count - 1; i >= 0; --i)
        {
            if (_nearTargets[i] == null)
            {
                _nearTargets.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 현재 수동 획득 가능한 타겟 중 가장 가까운 것을 찾는다.
    /// </summary>
    private CCollectableInteractObject2D GetClosestManualTarget()
    {
        CCollectableInteractObject2D closest = null;
        float closestSqrDistance = float.MaxValue;

        for (int i = 0; i < _nearTargets.Count; i++)
        {
            CCollectableInteractObject2D target = _nearTargets[i];

            if (target == null)
            {
                continue;
            }

            if (!target.CanManualCollect)
            {
                continue;
            }

            if (!target.IsCollectableNow)
            {
                continue;
            }

            Vector3 delta = target.transform.position - transform.position;
            float sqrDistance = delta.sqrMagnitude;

            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closest = target;
            }
        }

        return closest;
    }

    /// <summary>
    /// 현재 수동 상호작용 대상에 맞는 안내 문구를 갱신한다.
    /// </summary>
    private void RefreshInteractionUI(CCollectableInteractObject2D target)
    {
        if (_interactionText == null)
        {
            return;
        }

        if (target == null)
        {
            _interactionText.text = string.Empty;
            _interactionText.gameObject.SetActive(false);
            return;
        }

        _interactionText.gameObject.SetActive(true);
        _interactionText.text = target.GetInteractionMessage(_collectKey);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        var component = UObject.FindComponent<ItemCollectionCoordinator>(K.NAME_ITEM_COLLECT_ADMIN);
        if (component)
        {
            _collectionCoordinator = component;
        }
        else
        {
            UDebug.Print($"컴포넌트 Iteam Collection Coordinator을 씬에서 찾지 못했습니다.", LogType.Assert);
        }
    }

    private void Update()
    {
        CleanupNullTargets();

        CCollectableInteractObject2D manualTarget = GetClosestManualTarget();

        // 안내 UI 갱신
        RefreshInteractionUI(manualTarget);

        // 버튼 입력으로 채집
        if (manualTarget != null && Input.GetKeyDown(_collectKey))
        {
            manualTarget.TryCollect(this);
        }
    }

    private void OnDisable()
    {
        if (_interactionText != null)
        {
            _interactionText.text = string.Empty;
            _interactionText.gameObject.SetActive(false);
        }

        _nearTargets.Clear();
    }
    #endregion
}
