using UnityEngine;

/// <summary>
/// 낚시 포인트용 수동 상호작용 오브젝트.
/// 
/// 중요
/// - CFishingController2D 내부 enum(EFishingAreaType)에 직접 접근하지 않는다.
/// - 낚시 가능 여부 판단과 안내 문구는 CFishingController2D의 public 메서드만 사용한다.
/// - 이렇게 해야 private enum 접근 에러가 나지 않는다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CFishingSpotInteractObject2D : BaseMono, IInteractable
{
    [Header("기본 안내 문구")]
    [SerializeField] private string _defaultMessage = "낚시하기";

    [Header("상호작용 키 표시용")]
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;

    [Header("로그")]
    [SerializeField] private bool _logEnabled = true;

    public bool CanInteract(GameObject player)
    {
        CPlayerCollector2D collector = GetCollectorFromPlayer(player);
        if (collector == null)
        {
            return false;
        }

        if (collector.FishingController == null)
        {
            return false;
        }

        // public 메서드만 사용
        return collector.FishingController.CanManualFish(collector);
    }

    public void Interact(GameObject player)
    {
        CPlayerCollector2D collector = GetCollectorFromPlayer(player);
        if (collector == null)
        {
            if (_logEnabled)
            {
                Debug.LogWarning($"[CFishingSpotInteractObject2D] player에서 CPlayerCollector2D를 찾지 못했습니다. object={name}");
            }
            return;
        }

        bool result = collector.TryStartFishing();

        if (_logEnabled && !result)
        {
            Debug.Log($"[CFishingSpotInteractObject2D] 낚시 시작 실패. object={name}");
        }
    }

    public string GetMessage()
    {
        // IInteractable 시그니처상 player를 받을 수 없어서
        // 여기서는 고정 문구를 반환한다.
        return _defaultMessage;
    }

    /// <summary>
    /// 선택적으로 외부 UI에서 player 기준 상세 문구를 쓰고 싶을 때 호출
    /// </summary>
    public string GetDetailedMessage(GameObject player)
    {
        CPlayerCollector2D collector = GetCollectorFromPlayer(player);
        if (collector == null || collector.FishingController == null)
        {
            return _defaultMessage;
        }

        string message = collector.FishingController.GetInteractionMessage(_interactionKey, collector);
        return string.IsNullOrWhiteSpace(message) ? _defaultMessage : message;
    }

    private CPlayerCollector2D GetCollectorFromPlayer(GameObject player)
    {
        if (player == null)
        {
            return null;
        }

        CPlayerCollector2D collector = player.GetComponentInParent<CPlayerCollector2D>();
        if (collector == null)
        {
            collector = player.GetComponent<CPlayerCollector2D>();
        }

        return collector;
    }

    protected override void Awake()
    {
        base.Awake();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"[CFishingSpotInteractObject2D] {name}의 Collider2D가 Trigger가 아닙니다. 팀 상호작용 방식에 따라 확인이 필요합니다.");
        }
    }
}
