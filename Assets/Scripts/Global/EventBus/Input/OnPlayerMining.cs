using UnityEngine;

/// <summary>
/// 플레이어가 광석에 상호작용했을 때
/// </summary>
public readonly struct OnPlayerMining
{
    public readonly Vector2 orePos;

    public OnPlayerMining(Vector2 orePos)
    {
        this.orePos = orePos;
    }

    /// <param name="orePos">나무 좌표</param>
    public static void Publish(Vector2 orePos)
    {
        EventBus<OnPlayerMining>.Publish(new OnPlayerMining(orePos));
    }
}
