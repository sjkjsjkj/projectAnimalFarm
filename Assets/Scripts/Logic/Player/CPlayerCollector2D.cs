using TMPro;
using UnityEngine;

/// <summary>
/// 플레이어가 채집 / 낚시 / 수확 등의 보상을 실제로 받는 창구.
/// 
/// 이 스크립트의 역할
/// 1. 상호작용 대상 오브젝트가 플레이어에게 아이템 지급 요청을 하면 받아준다.
/// 2. 실제 아이템 지급은 ItemCollectionCoordinator에게 넘긴다.
/// 3. 상호작용 중 Busy 상태를 관리한다.
/// 4. 채집 / 낚시 애니메이션을 재생한다.
/// 5. 생활 스킬 경험치를 지급한다.
/// 6. 낚시 포인트가 요청하면 CFishingController2D를 통해 낚시를 시작한다.
/// 
/// 중요
/// - 팀 상호작용 시스템을 대체하는 스크립트가 아니다.
/// - 팀 상호작용 시스템이 "Interact(player)"를 호출한 뒤
///   플레이어 쪽에서 실제 처리만 담당하는 보조 창구이다.
/// </summary>
public class CPlayerCollector2D : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("아이템 지급 관리자")]
    [Tooltip("실제 아이템을 인벤토리에 넣고 도감을 해금해주는 허브")]
    [SerializeField] private ItemCollectionCoordinator _collectionCoordinator;

    [Header("낚시 컨트롤러")]
    [Tooltip("플레이어 기준 낚시 처리용 컨트롤러")]
    [SerializeField] private CFishingController2D _fishingController;

    [Header("상호작용 연출")]
    [Tooltip("상호작용 시 사용할 애니메이터")]
    [SerializeField] private Animator _interactionAnimator;

    [Tooltip("Busy 상태를 제어할 애니메이터 Bool 이름")]
    [SerializeField] private string _busyBoolName = "IsBusy";

    [Tooltip("채집 애니메이션 Trigger 이름")]
    [SerializeField] private string _gatherTriggerName = "Gather";

    [Tooltip("낚시 애니메이션 Trigger 이름")]
    [SerializeField] private string _fishingTriggerName = "Fish";

    [Tooltip("Busy 중 잠깐 비활성화할 컴포넌트들\n예: 이동 스크립트, 입력 스크립트")]
    [SerializeField] private MonoBehaviour[] _disableTargetsWhileBusy;

    [Header("생활 스킬 데이터")]
    [Tooltip("생활 스킬 경험치 지급 시 사용할 플레이어 월드 SO")]
    [SerializeField] private PlayerWorldSO _playerWorldSo;

    [Header("선택 UI")]
    [Tooltip("상호작용 안내 문구를 직접 표시할 UI 텍스트가 있으면 연결")]
    [SerializeField] private TMP_Text _interactionText;

    [Header("로그")]
    [Tooltip("동작 확인용 로그 출력 여부")]
    [SerializeField] private bool _logEnabled = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    /// <summary>
    /// 현재 플레이어가 상호작용 중인지 여부
    /// true면 다른 상호작용을 막을 수 있다.
    /// </summary>
    private bool _isBusy = false;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public bool IsBusy => _isBusy;
    public CFishingController2D FishingController => _fishingController;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 실제 아이템을 지급받는 함수.
    /// 
    /// 채집 오브젝트 / 낚시 보상 / 나중의 농작물 수확 등이
    /// 결국 이 함수를 통해 플레이어에게 아이템을 줄 수 있다.
    /// </summary>
    public bool TryReceiveItem(string itemId, int amount)
    {
        // [수정 포인트 1]
        // 허브 스크립트 연결 여부 검사
        if (_collectionCoordinator == null)
        {
            Debug.LogError("[CPlayerCollector2D] ItemCollectionCoordinator가 연결되지 않았습니다.");
            return false;
        }

        // 잘못된 값 방어
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

        // [수정 포인트 2]
        // 실제 인벤토리 추가 / 도감 해금은 ItemCollectionCoordinator에게 맡긴다.
        return _collectionCoordinator.TryCollectItem(itemId, amount);
    }

    /// <summary>
    /// 플레이어가 새 상호작용을 시작할 수 있는 상태인지 확인
    /// 
    /// 현재는 Busy 여부만 기준으로 본다.
    /// 필요하면 나중에 기절, 대화 중, 컷신 중 같은 조건도 여기에 추가 가능하다.
    /// </summary>
    public bool CanStartInteraction()
    {
        return !_isBusy;
    }

    /// <summary>
    /// 상호작용 중 상태를 켜고 끄는 함수
    /// 
    /// 이 함수가 하는 일
    /// 1. 내부 Busy 상태 변경
    /// 2. 애니메이터 Bool 반영
    /// 3. 이동/입력 스크립트 잠깐 비활성화
    /// 4. 상호작용 UI 정리
    /// </summary>
    public void SetInteractionBusy(bool isBusy)
    {
        // 같은 상태면 굳이 다시 처리하지 않음
        if (_isBusy == isBusy)
        {
            return;
        }

        _isBusy = isBusy;

        // [수정 포인트 3]
        // 애니메이터 Busy Bool 반영
        if (_interactionAnimator != null && !string.IsNullOrWhiteSpace(_busyBoolName))
        {
            _interactionAnimator.SetBool(_busyBoolName, _isBusy);
        }

        // [수정 포인트 4]
        // Busy 중 잠깐 꺼둘 컴포넌트들 제어
        if (_disableTargetsWhileBusy != null)
        {
            for (int i = 0; i < _disableTargetsWhileBusy.Length; i++)
            {
                MonoBehaviour target = _disableTargetsWhileBusy[i];

                if (target == null)
                {
                    continue;
                }

                // 자기 자신은 꺼버리면 안 되므로 제외
                if (target == this)
                {
                    continue;
                }

                // Busy면 false, 아니면 true
                target.enabled = !_isBusy;
            }
        }

        // Busy 시작할 때 상호작용 문구가 있으면 숨김
        if (_interactionText != null && _isBusy)
        {
            _interactionText.text = string.Empty;
            _interactionText.gameObject.SetActive(false);
        }

        if (_logEnabled)
        {
            Debug.Log($"[CPlayerCollector2D] Busy 상태 변경: {_isBusy}");
        }
    }

    /// <summary>
    /// 채집 애니메이션 재생
    /// </summary>
    public void PlayGatherAnimation()
    {
        PlayAnimationTrigger(_gatherTriggerName);
    }

    /// <summary>
    /// 낚시 애니메이션 재생
    /// </summary>
    public void PlayFishingAnimation()
    {
        PlayAnimationTrigger(_fishingTriggerName);
    }

    /// <summary>
    /// 생활 스킬 경험치 지급
    /// 
    /// 예:
    /// - 채집 성공 시 Gathering 경험치
    /// - 낚시 성공 시 Fishing 경험치
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

        // [수정 포인트 5]
        // 경험치 지급에 필요한 참조들이 없으면 조용히 스킵
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
            Debug.Log($"[CPlayerCollector2D] 생활 스킬 경험치 지급: skill={skill}, exp={expAmount}");
        }
    }

    /// <summary>
    /// 낚시 포인트 오브젝트가 플레이어에게 낚시를 시도시킬 때 사용하는 함수
    /// </summary>
    public bool TryStartFishing()
    {
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

    /// <summary>
    /// 외부에서 상호작용 안내 문구를 직접 띄우고 싶을 때 사용하는 함수
    /// 
    /// 팀 상호작용 UI를 쓴다면 굳이 안 써도 된다.
    /// </summary>
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

    /// <summary>
    /// 상호작용 안내 문구 숨김
    /// </summary>
    public void ClearInteractionText()
    {
        if (_interactionText == null)
        {
            return;
        }

        _interactionText.text = string.Empty;
        _interactionText.gameObject.SetActive(false);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 애니메이터 Trigger 실행 공용 함수
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

    /// <summary>
    /// 씬에서 ItemCollectionCoordinator를 자동으로 찾아오는 함수
    /// </summary>
    private void AutoBindCollectionCoordinator()
    {
        if (_collectionCoordinator != null)
        {
            return;
        }

        // 프로젝트에서 쓰던 방식 우선 사용
        ItemCollectionCoordinator component = UObject.FindComponent<ItemCollectionCoordinator>(K.NAME_ITEM_COLLECT_ADMIN);

        if (component != null)
        {
            _collectionCoordinator = component;
            return;
        }

        // 방어적으로 한 번 더 찾기
        _collectionCoordinator = FindAnyObjectByType<ItemCollectionCoordinator>();
    }

    /// <summary>
    /// 플레이어 본인에게 붙은 낚시 컨트롤러 자동 바인딩
    /// </summary>
    private void AutoBindFishingController()
    {
        if (_fishingController != null)
        {
            return;
        }

        _fishingController = GetComponent<CFishingController2D>();
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        // [수정 포인트 6]
        // 인스펙터 연결 실수 방지용 자동 바인딩
        AutoBindCollectionCoordinator();
        AutoBindFishingController();

        if (_collectionCoordinator == null)
        {
            UDebug.Print("CPlayerCollector2D : ItemCollectionCoordinator를 찾지 못했습니다.", LogType.Assert);
        }

        if (_fishingController == null && _logEnabled)
        {
            Debug.LogWarning("[CPlayerCollector2D] CFishingController2D가 연결되지 않았습니다. 낚시 기능이 필요하면 연결하세요.");
        }
    }

    private void OnDisable()
    {
        // 상호작용 문구 정리
        ClearInteractionText();

        // Busy 상태 초기화
        _isBusy = false;

        // Busy 상태로 꺼두었던 컴포넌트 복구
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

        // 애니메이터 Bool도 원복
        if (_interactionAnimator != null && !string.IsNullOrWhiteSpace(_busyBoolName))
        {
            _interactionAnimator.SetBool(_busyBoolName, false);
        }
    }
    #endregion
}
