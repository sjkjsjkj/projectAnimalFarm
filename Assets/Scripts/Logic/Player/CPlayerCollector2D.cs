using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 플레이어가 채집 / 낚시 / 수확 등의 보상을 실제로 받는 창구.
/// 
/// 핵심 수정
/// - Collection Coordinator를 수동 연결에만 의존하지 않음
/// - 플레이 시작 시점에 런타임 오브젝트가 아직 없을 수 있으므로 코루틴으로 재탐색
/// - TryReceiveItem 호출 시점에도 한 번 더 안전하게 재탐색
/// 
/// 이걸로 해결하려는 문제
/// - 플레이 시작하면 Inspector에 Collection Coordinator가 Missing 으로 바뀌는 현상
/// - 런타임에 생성된 플레이어 / 씬 오브젝트 타이밍 차이 때문에 참조가 끊기는 현상
/// </summary>
public class CPlayerCollector2D : BaseMono
{
    [Header("아이템 지급 관리자")]
    [SerializeField] private ItemCollectionCoordinator _collectionCoordinator;

    [Header("낚시 컨트롤러")]
    [SerializeField] private CFishingController2D _fishingController;

    [Header("상호작용 연출")]
    [SerializeField] private Animator _interactionAnimator;
    [SerializeField] private string _busyBoolName = "IsBusy";
    [SerializeField] private string _gatherTriggerName = "Gather";
    [SerializeField] private string _fishingTriggerName = "Fish";
    [SerializeField] private MonoBehaviour[] _disableTargetsWhileBusy;

    [Header("생활 스킬 데이터")]
    [SerializeField] private PlayerWorldSO _playerWorldSo;

    [Header("선택 UI")]
    [SerializeField] private TMP_Text _interactionText;

    [Header("자동 탐색 옵션")]
    [SerializeField] private bool _autoFindCollectionCoordinator = true;
    [SerializeField] private float _collectionCoordinatorFindTimeout = 10f;

    [Header("로그")]
    [SerializeField] private bool _logEnabled = true;

    private bool _isBusy = false;

    public bool IsBusy => _isBusy;
    public CFishingController2D FishingController => _fishingController;

    protected override void Awake()
    {
        base.Awake();

        AutoBindFishingController();
        TryResolveCollectionCoordinator();

        if (_autoFindCollectionCoordinator && _collectionCoordinator == null)
        {
            StartCoroutine(CoFindCollectionCoordinator());
        }
    }

    public bool TryReceiveItem(string itemId, int amount)
    {
        if (_collectionCoordinator == null)
        {
            TryResolveCollectionCoordinator();
        }

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
            Debug.LogWarning("[CPlayerCollector2D] amount가 0 이하입니다. itemId=" + itemId + ", amount=" + amount);
            return false;
        }

        return _collectionCoordinator.TryCollectItem(itemId, amount);
    }

    public bool CanStartInteraction()
    {
        return !_isBusy;
    }

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

                target.enabled = !_isBusy;
            }
        }

        if (_interactionText != null && _isBusy)
        {
            _interactionText.text = string.Empty;
            _interactionText.gameObject.SetActive(false);
        }

        if (_logEnabled)
        {
            Debug.Log("[CPlayerCollector2D] Busy 상태 변경: " + _isBusy);
        }
    }

    public void PlayGatherAnimation()
    {
        PlayAnimationTrigger(_gatherTriggerName);
    }

    public void PlayFishingAnimation()
    {
        PlayAnimationTrigger(_fishingTriggerName);
    }

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
                Debug.LogWarning("[CPlayerCollector2D] PlayerWorldSO가 연결되지 않아 생활 스킬 경험치 지급을 건너뜁니다.");
            }
            return;
        }

        if (DataManager.Ins == null || DataManager.Ins.Player == null)
        {
            if (_logEnabled)
            {
                Debug.LogWarning("[CPlayerCollector2D] DataManager 또는 PlayerProvider가 없어 생활 스킬 경험치 지급을 건너뜁니다.");
            }
            return;
        }

        DataManager.Ins.Player.AddSkillExp(skill, expAmount, _playerWorldSo);

        if (_logEnabled)
        {
            Debug.Log("[CPlayerCollector2D] 생활 스킬 경험치 지급: skill=" + skill + ", exp=" + expAmount);
        }
    }

    public bool TryStartFishing()
    {
        if (_fishingController == null)
        {
            AutoBindFishingController();
        }

        if (_fishingController == null)
        {
            if (_logEnabled)
            {
                Debug.LogWarning("[CPlayerCollector2D] CFishingController2D가 연결되지 않았습니다.");
            }
            return false;
        }

        return _fishingController.TryFish(this);
    }

    public void SetInteractionText(string message)
    {
        if (_interactionText == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            _interactionText.text = string.Empty;
            _interactionText.gameObject.SetActive(false);
            return;
        }

        _interactionText.text = message;
        _interactionText.gameObject.SetActive(true);
    }

    public void ClearInteractionText()
    {
        if (_interactionText == null)
        {
            return;
        }

        _interactionText.text = string.Empty;
        _interactionText.gameObject.SetActive(false);
    }

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

    private void AutoBindFishingController()
    {
        if (_fishingController != null)
        {
            return;
        }

        _fishingController = GetComponent<CFishingController2D>();

        if (_fishingController == null)
        {
            _fishingController = GetComponentInParent<CFishingController2D>();
        }
    }

    private void TryResolveCollectionCoordinator()
    {
        if (_collectionCoordinator != null)
        {
            return;
        }

        _collectionCoordinator = FindObjectOfType<ItemCollectionCoordinator>(true);

        if (_collectionCoordinator != null && _logEnabled)
        {
            Debug.Log("[CPlayerCollector2D] ItemCollectionCoordinator 자동 연결 완료: " + _collectionCoordinator.name);
        }
    }

    private IEnumerator CoFindCollectionCoordinator()
    {
        float elapsed = 0f;

        while (elapsed < _collectionCoordinatorFindTimeout)
        {
            if (_collectionCoordinator == null)
            {
                TryResolveCollectionCoordinator();
            }

            if (_collectionCoordinator != null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_logEnabled)
        {
            Debug.LogWarning("[CPlayerCollector2D] 제한 시간 내에 ItemCollectionCoordinator를 찾지 못했습니다.");
        }
    }

    private void OnDisable()
    {
        ClearInteractionText();

        _isBusy = false;

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

                target.enabled = true;
            }
        }

        if (_interactionAnimator != null && !string.IsNullOrWhiteSpace(_busyBoolName))
        {
            _interactionAnimator.SetBool(_busyBoolName, false);
        }
    }
}
