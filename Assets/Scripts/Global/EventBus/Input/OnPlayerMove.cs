using UnityEngine;

/// <summary>
/// 플레이어가 WASD를 입력했을 때
/// 누르는 동안 이벤트를 계속 발행
/// </summary>
public readonly struct OnPlayerMove
{
    public readonly Vector2 moved;

    public OnPlayerMove(Vector2 moved)
    {
        this.moved = moved;
    }

    /// <param name="moved">이동량</param>
    public static void Publish(Vector2 moved)
    {
        EventBus<OnPlayerMove>.Publish(new OnPlayerMove(moved));
    }
}
