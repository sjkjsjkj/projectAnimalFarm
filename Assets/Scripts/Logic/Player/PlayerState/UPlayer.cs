using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public static class UPlayer
{
    /// <summary>
    /// 방향 백터를 BlendTree용 Float 값으로 변환합니다.
    /// (0 : Down / 0.5 : Side / 1 : Up)
    /// </summary>
    /// <param name="dir">방향 또는 속도</param>
    /// <returns></returns>
    public static float GetFacingValue(Vector2 dir)
    {
        // 양측 이동을 판단
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return 0.5f; // Side
        }
        // 위아래 이동일 경우
        return dir.y > 0 ? 1f : 0f; // Up, Down
    }

    /// <summary>
    /// BlendTree용 Float 값으로 이미지 방향을 설정합니다.
    /// (0 : Down / 0.5 : Side / 1 : Up)
    /// </summary>
    /// <param name="dir">방향 또는 속도</param>
    public static void SetSpriteFacing(SpriteRenderer sprite, Vector2 dir)
    {
        // 양쪽 이동일 경우 이미지 방향 설정
        if (Mathf.Abs(dir.x) > K.SMALL_DISTANCE)
        {
            sprite.flipX = dir.x < 0f; // 방향 설정
        }
    }
}
