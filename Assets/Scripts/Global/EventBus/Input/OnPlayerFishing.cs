using UnityEngine;

/// <summary>
/// 플레이어가 낚시 포인트에 상호작용했을 때
/// </summary>
public readonly struct OnPlayerFishing
{
    public readonly Vector2 fishingPointPos;

    public OnPlayerFishing(Vector2 fishingPointPos)
    {
        this.fishingPointPos = fishingPointPos;
    }

    /// <param name="fishingPointPos">나무 좌표</param>
    public static void Publish(Vector2 fishingPointPos)
    {
        EventBus<OnPlayerFishing>.Publish(new OnPlayerFishing(fishingPointPos));
    }
}
