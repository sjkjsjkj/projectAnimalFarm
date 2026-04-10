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


    private static string[] _grassStepArr =
    {
        Id.Sfx_Environment_StepGrass_1,
        Id.Sfx_Environment_StepGrass_2,
        Id.Sfx_Environment_StepGrass_3,
        Id.Sfx_Environment_StepGrass_4,
        Id.Sfx_Environment_StepGrass_5,
        Id.Sfx_Environment_StepGrass_6,
    };
    private static string[] _coralStepArr =
    {
        Id.Sfx_Environment_StepCoral_1,
        Id.Sfx_Environment_StepCoral_2,
        Id.Sfx_Environment_StepCoral_3,
        Id.Sfx_Environment_StepCoral_4,
        Id.Sfx_Environment_StepCoral_5,
        Id.Sfx_Environment_StepCoral_6,
    };
    private static string[] _clothStepArr =
    {
        Id.Sfx_Environment_StepCloth_1,
        Id.Sfx_Environment_StepCloth_2,
        Id.Sfx_Environment_StepCloth_3,
        Id.Sfx_Environment_StepCloth_4,
    };
    private static string[] _ladderStepArr =
    {
        Id.Sfx_Environment_StepLadder_1,
        Id.Sfx_Environment_StepLadder_2,
        Id.Sfx_Environment_StepLadder_3,
        Id.Sfx_Environment_StepLadder_4,
        Id.Sfx_Environment_StepLadder_5,
    };
    private static string[] _gravelStepArr =
    {
        Id.Sfx_Environment_StepGravel_1,
        Id.Sfx_Environment_StepGravel_2,
        Id.Sfx_Environment_StepGravel_3,
        Id.Sfx_Environment_StepGravel_4,
    };
    private static string[] _stoneStepArr =
    {
        Id.Sfx_Environment_StepStone_1,
        Id.Sfx_Environment_StepStone_2,
        Id.Sfx_Environment_StepStone_3,
        Id.Sfx_Environment_StepStone_4,
        Id.Sfx_Environment_StepStone_5,
        Id.Sfx_Environment_StepStone_6,
    };
    /// <summary>
    /// 카메라에서 발자국 소리를 재생합니다.
    /// </summary>
    public static void TryPlayStepSound(ref float nextStepTime, float stepInterval)
    {
        if (UMath.TryCooldownEnd(Time.time, ref nextStepTime, stepInterval))
        {
            // 현재 only grass
            int index = Random.Range(0, _grassStepArr.Length);
            USound.PlaySfx(_grassStepArr[index]);
        }
    }
}
