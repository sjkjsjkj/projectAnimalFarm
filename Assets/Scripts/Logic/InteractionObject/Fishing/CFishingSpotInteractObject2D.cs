using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CFishingSpotInteractObject2D : BaseMono, IInteractable
{
    public enum EFishingSpotType
    {
        FreshWater = 0,
        SeaWater
    }

    [Header("낚시 스팟 타입")]
    [SerializeField] private EFishingSpotType _fishingSpotType = EFishingSpotType.FreshWater;

    [Header("메시지")]
    [SerializeField] private string _defaultMessage = "낚시하기";

    [Header("상호작용 키 표시용")]
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;

    [Header("로그")]
    [SerializeField] private bool _logEnabled = true;

    public bool CanInteract(GameObject player)
    {
        if (_logEnabled)
        {
            Debug.Log("[FishingSpot] CanInteract 호출됨");
        }

        CPlayerCollector2D collector = GetCollectorFromPlayer(player);

        if (collector == null)
        {
            if (_logEnabled)
            {
                Debug.Log("[FishingSpot] CanInteract 실패: collector 없음");
            }
            return false;
        }

        if (collector.FishingController == null)
        {
            if (_logEnabled)
            {
                Debug.Log("[FishingSpot] CanInteract 실패: FishingController 없음");
            }
            return false;
        }

        // 중요:
        // 여기서 낚시 가능 여부를 너무 빡세게 검사하면
        // TryFishFromSpot()까지 못 들어가서 실패 피드백 메시지가 안 뜬다.
        // 실제 성공/실패 판정은 FishingController 내부에서 처리하게 둔다.
        return true;
    }

    public void Interact(GameObject player)
    {
        if (_logEnabled)
        {
            Debug.Log("[FishingSpot] Interact 호출됨");
        }

        CPlayerCollector2D collector = GetCollectorFromPlayer(player);

        if (collector == null)
        {
            if (_logEnabled)
            {
                Debug.Log("[FishingSpot] Interact 실패: collector 없음");
            }
            return;
        }

        if (collector.FishingController == null)
        {
            if (_logEnabled)
            {
                Debug.Log("[FishingSpot] Interact 실패: FishingController 없음");
            }
            return;
        }

        bool useSeaFishing = _fishingSpotType == EFishingSpotType.SeaWater;

        bool result = collector.FishingController.TryFishFromSpot(
            collector,
            useSeaFishing,
            transform.position
        );

        if (_logEnabled)
        {
            Debug.Log("[FishingSpot] TryFishFromSpot 결과 = " + result);
        }
    }

    public string GetMessage()
    {
        if (!string.IsNullOrWhiteSpace(_defaultMessage))
        {
            return _defaultMessage;
        }

        return _fishingSpotType == EFishingSpotType.SeaWater ? "바다 낚시하기" : "낚시하기";
    }

    public string GetDetailedMessage(GameObject player)
    {
        CPlayerCollector2D collector = GetCollectorFromPlayer(player);

        if (collector != null && collector.FishingController != null)
        {
            string detailMessage = collector.FishingController.GetInteractionMessage(_interactionKey, collector);

            if (!string.IsNullOrWhiteSpace(detailMessage))
            {
                return detailMessage;
            }
        }

        return GetMessage();
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
            Debug.LogWarning("[FishingSpot] " + name + " Collider2D가 Trigger가 아닙니다.");
        }
    }
}
