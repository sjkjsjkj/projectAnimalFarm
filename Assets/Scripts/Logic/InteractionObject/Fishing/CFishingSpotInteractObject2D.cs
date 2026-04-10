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
                Debug.Log("[FishingSpot] collector 없음");
            }
            return false;
        }

        if (collector.FishingController == null)
        {
            if (_logEnabled)
            {
                Debug.Log("[FishingSpot] FishingController 없음");
            }
            return false;
        }

        bool useSeaFishing = _fishingSpotType == EFishingSpotType.SeaWater;
        bool canFish = collector.FishingController.CanManualFishFromSpot(
            collector,
            useSeaFishing,
            transform.position
        );

        if (_logEnabled)
        {
            Debug.Log("[FishingSpot] CanManualFish 결과 = " + canFish);
        }

        return canFish;
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

        bool useSeaFishing = _fishingSpotType == EFishingSpotType.SeaWater;
        bool result = collector.FishingController != null &&
                      collector.FishingController.TryFishFromSpot(
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
