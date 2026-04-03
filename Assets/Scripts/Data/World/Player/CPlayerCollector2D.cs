using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 플레이어가 주변 채집 오브젝트 / 낚시와 상호작용할 수 있게 해주는 스크립트
/// - 버튼 입력(E키 등)으로 수동 채집
/// - 물가 타일 앞에서 버튼 입력으로 낚시
/// - ItemCollectionCoordinator를 통해 인벤토리/도감에 반영
/// - 상호작용 중에는 플레이어를 Busy 상태로 두어 중복 입력을 막음
/// </summary>
public class CPlayerCollector2D : BaseMono
{
    private enum EInteractionViewType
    {
        None = 0,
        Gathering,
        Fishing
    }

    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("입력 설정")]
    [SerializeField] private KeyCode _collectKey = KeyCode.E;

    [Header("아이템 지급 관리자")]
    [SerializeField] private ItemCollectionCoordinator _collectionCoordinator;

    [Header("인터렉션 안내 UI (선택)")]
    [SerializeField] private TMP_Text _interactionText;

    [Header("낚시 컨트롤러 (선택)")]
    [SerializeField] private CFishingController2D _fishingController;

    [Header("상호작용 연출 (선택)")]
    [SerializeField] private Animator _interactionAnimator;
    [SerializeField] private string _busyBoolName = "IsBusy";
    [SerializeField] private string _gatherTriggerName = "Gather";
    [SerializeField] private string _fishingTriggerName = "Fish";

    [Tooltip("상호작용 중 잠깐 꺼둘 컴포넌트들\n예: PlayerController")]
    [SerializeField] private MonoBehaviour[] _disableTargetsWhileBusy;

    [Header("생활 스킬 데이터 (선택)")]
    [Tooltip("생활 스킬 경험치를 올릴 때 사용할 플레이어 SO\n연결하지 않으면 경험치 지급은 생략됨")]
    [SerializeField] private PlayerWorldSO _playerWorldSo;

    [Header("로그 출력")]
    [SerializeField] private bool _logEnabled = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    // 현재 플레이어 근처에 들어와 있는 채집 가능한 오브젝트 목록
    private readonly List<CCollectableInteractObject2D> _nearTargets = new List<CCollectableInteractObject2D>();

    // 현재 플레이어가 상호작용 중인지 여부
    private bool _isBusy = false;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public KeyCode CollectKey => _collectKey;
    public bool IsBusy => _isBusy;
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
            Debug.Log($"타겟 등록: {target.name}");
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
            Debug.Log($"타겟 해제: {target.name}");
        }
    }

    /// <summary>
    /// 실제 아이템을 지급받는 함수
    /// 채집 오브젝트 / 낚시 컨트롤러가 이 함수를 호출해서 아이템을 플레이어에게 준다.
    /// </summary>
    public bool TryReceiveItem(string itemId, int amount)
    {
        if (_collectionCoordinator == null)
        {
            Debug.LogError("ItemCollectionCoordinator가 연결되지 않았습니다.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogWarning("itemId가 비어 있습니다.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"amount가 0 이하입니다. itemId={itemId}, amount={amount}");
            return false;
        }

        return _collectionCoordinator.TryCollectItem(itemId, amount);
    }

    /// <summary>
    /// 현재 플레이어가 새 상호작용을 시작 가능한 상태인지 반환
    /// </summary>
    public bool CanStartInteraction()
    {
        return !_isBusy;
    }

    /// <summary>
    /// 상호작용 중 상태를 켜고 끈다.
    /// - 이동 스크립트 등 잠깐 막고 싶을 때 사용
    /// - 중복 입력 방지용
    /// </summary>
    public void SetInteractionBusy(bool isBusy)
    {
        if (_isBusy == isBusy)
        {
            return;
        }

        _isBusy = isBusy;

        if (_interactionAnimator != null && !string.IsNullOrWhiteSpace(_busyBoolName))
        {
            _interactionAnimator.SetBool(_busyBoolName, _isBusy);
        }

        if (_disableTargetsWhileBusy != null)
        {
            for (int i = 0; i < _disableTargetsWhileBusy.Length; i++)
            {
                MonoBehaviour target = _disableTargetsWhileBusy[i];

                if (target == null)
                {
                    continue;
                }

                if (target == this)
                {
                    continue;
                }

                // Busy 상태일 때 false, 아니면 true
                target.enabled = !_isBusy;
            }
        }

        // Busy 상태가 켜지면 UI는 바로 숨김
        if (_isBusy)
        {
            RefreshInteractionUI(null, EInteractionViewType.None);
        }

        if (_logEnabled)
        {
            Debug.Log($"Busy 상태 변경: {_isBusy}");
        }
    }

    /// <summary>
    /// 채집 애니메이션 트리거 실행
    /// </summary>
    public void PlayGatherAnimation()
    {
        PlayAnimationTrigger(_gatherTriggerName);
    }

    /// <summary>
    /// 낚시 애니메이션 트리거 실행
    /// </summary>
    public void PlayFishingAnimation()
    {
        PlayAnimationTrigger(_fishingTriggerName);
    }

    /// <summary>
    /// 생활 스킬 경험치 지급
    /// 연결이 안 되어 있으면 그냥 조용히 스킵하도록 만들어 둠
    /// </summary>
    public void TryAddLifeSkillExp(ELifeSkill skill, int expAmount)
    {
        if (skill == ELifeSkill.None)
        {
            return;
        }

        if (expAmount <= 0)
        {
            return;
        }

        if (_playerWorldSo == null)
        {
            if (_logEnabled)
            {
                Debug.LogWarning("PlayerWorldSO가 연결되지 않아 생활 스킬 경험치 지급을 건너뜁니다.");
            }
            return;
        }

        if (DataManager.Ins == null || DataManager.Ins.Player == null)
        {
            if (_logEnabled)
            {
                Debug.LogWarning("DataManager 또는 PlayerProvider가 없어 생활 스킬 경험치 지급을 건너뜁니다.");
            }
            return;
        }

        DataManager.Ins.Player.AddSkillExp(skill, expAmount, _playerWorldSo);

        if (_logEnabled)
        {
            Debug.Log($"생활 스킬 경험치 지급: skill={skill}, exp={expAmount}");
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 파괴되었거나 비활성화된 타겟 제거
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

            if (!target.CanCollect(this))
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
    /// 안내 문구 갱신
    /// 우선순위
    /// 1. 채집 타겟
    /// 2. 낚시 가능 여부
    /// 3. 아무것도 없으면 숨김
    /// </summary>
    private void RefreshInteractionUI(CCollectableInteractObject2D target, EInteractionViewType viewType)
    {
        if (_interactionText == null)
        {
            return;
        }

        if (_isBusy)
        {
            _interactionText.text = string.Empty;
            _interactionText.gameObject.SetActive(false);
            return;
        }

        switch (viewType)
        {
            case EInteractionViewType.Gathering:
                if (target == null)
                {
                    _interactionText.text = string.Empty;
                    _interactionText.gameObject.SetActive(false);
                    return;
                }

                _interactionText.gameObject.SetActive(true);
                _interactionText.text = target.GetInteractionMessage(_collectKey);
                break;

            case EInteractionViewType.Fishing:
                if (_fishingController == null)
                {
                    _interactionText.text = string.Empty;
                    _interactionText.gameObject.SetActive(false);
                    return;
                }

                _interactionText.gameObject.SetActive(true);
                _interactionText.text = _fishingController.GetInteractionMessage(_collectKey, this);
                break;

            default:
                _interactionText.text = string.Empty;
                _interactionText.gameObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// 애니메이터 트리거 실행
    /// </summary>
    private void PlayAnimationTrigger(string triggerName)
    {
        if (_interactionAnimator == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(triggerName))
        {
            return;
        }

        _interactionAnimator.ResetTrigger(triggerName);
        _interactionAnimator.SetTrigger(triggerName);
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
        if (_fishingController == null)
        {
            _fishingController = GetComponent<CFishingController2D>();
        }
    }

    private void Update()
    {
        CleanupNullTargets();

        // 상호작용 중이면 안내 UI를 숨기고 새 입력을 막는다.
        if (_isBusy)
        {
            RefreshInteractionUI(null, EInteractionViewType.None);
            return;
        }

        CCollectableInteractObject2D manualTarget = GetClosestManualTarget();

        // UI 우선순위
        if (manualTarget != null)
        {
            RefreshInteractionUI(manualTarget, EInteractionViewType.Gathering);
        }
        else if (_fishingController != null && _fishingController.CanManualFish(this))
        {
            RefreshInteractionUI(null, EInteractionViewType.Fishing);
        }
        else
        {
            RefreshInteractionUI(null, EInteractionViewType.None);
        }

        // 입력 처리
        if (Input.GetKeyDown(_collectKey))
        {
            // 1순위 : 채집
            if (manualTarget != null)
            {
                manualTarget.TryCollect(this);
                return;
            }

            // 2순위 : 낚시
            if (_fishingController != null)
            {
                _fishingController.TryFish(this);
            }
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
        _isBusy = false;
    }
    #endregion
}
