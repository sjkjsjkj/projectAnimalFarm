using UnityEngine;

/// <summary>
/// 플레이어가 나무에 상호작용했을 때
/// </summary>
public readonly struct OnPlayerLogging
{
    public readonly Vector2 woodPos;

    public OnPlayerLogging(Vector2 woodPos)
    {
        this.woodPos = woodPos;
    }

    /// <param name="woodPos">나무 좌표</param>
    public static void Publish(Vector2 woodPos)
    {
        EventBus<OnPlayerLogging>.Publish(new OnPlayerLogging(woodPos));
    }
}
