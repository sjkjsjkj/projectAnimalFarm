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

    // 동굴 ─ 바닥
    private static string[] _stoneStepArr =
    {
        Id.Sfx_Environment_StepStone_1,
        Id.Sfx_Environment_StepStone_2,
        Id.Sfx_Environment_StepStone_3,
        Id.Sfx_Environment_StepStone_4,
        Id.Sfx_Environment_StepStone_5,
        Id.Sfx_Environment_StepStone_6,
    };
    // 동굴 ─ 레일
    private static string[] _woodStepArr =
    {
        Id.Sfx_Environment_StepWood_1,
        Id.Sfx_Environment_StepWood_2,
        Id.Sfx_Environment_StepWood_3,
        Id.Sfx_Environment_StepWood_4,
        Id.Sfx_Environment_StepWood_5,
        Id.Sfx_Environment_StepWood_6,
    };
    // 숲 ─ 잔디
    private static string[] _wetGrassStepArr =
    {
        Id.Sfx_Environment_StepWetGrass_1,
        Id.Sfx_Environment_StepWetGrass_2,
        Id.Sfx_Environment_StepWetGrass_3,
        Id.Sfx_Environment_StepWetGrass_4,
        Id.Sfx_Environment_StepWetGrass_5,
        Id.Sfx_Environment_StepWetGrass_6,
    };
    // 숲, 타운 ─ 목재 다리
    private static string[] _ladderStepArr =
    {
        Id.Sfx_Environment_StepLadder_1,
        Id.Sfx_Environment_StepLadder_2,
        Id.Sfx_Environment_StepLadder_3,
        Id.Sfx_Environment_StepLadder_4,
        Id.Sfx_Environment_StepLadder_5,
    };
    // 타운 ─ 돌 길 (돌 다리, 돌 계단, 조약돌)
    private static string[] _scaffoldStepArr =
    {
        Id.Sfx_Environment_StepScaffold_1,
        Id.Sfx_Environment_StepScaffold_2,
        Id.Sfx_Environment_StepScaffold_3,
        Id.Sfx_Environment_StepScaffold_4,
        Id.Sfx_Environment_StepScaffold_5,
        Id.Sfx_Environment_StepScaffold_6,
        Id.Sfx_Environment_StepScaffold_7,
    };
    // 타운 ─ 마을 잔디
    private static string[] _grassStepArr =
    {
        Id.Sfx_Environment_StepGrass_1,
        Id.Sfx_Environment_StepGrass_2,
        Id.Sfx_Environment_StepGrass_3,
        Id.Sfx_Environment_StepGrass_4,
        Id.Sfx_Environment_StepGrass_5,
        Id.Sfx_Environment_StepGrass_6,
    };
    // 타운 ─ 꽃이 무성한 잔디
    private static string[] _clothStepArr =
    {
        Id.Sfx_Environment_StepCloth_1,
        Id.Sfx_Environment_StepCloth_2,
        Id.Sfx_Environment_StepCloth_3,
        Id.Sfx_Environment_StepCloth_4,
    };
    // 타운 ─ 일반 흙
    private static string[] _gravelStepArr =
    {
        Id.Sfx_Environment_StepGravel_1,
        Id.Sfx_Environment_StepGravel_2,
        Id.Sfx_Environment_StepGravel_3,
        Id.Sfx_Environment_StepGravel_4,
    };
    // 타운 ─ 밭 흙
    private static string[] _tuffStepArr =
    {
        Id.Sfx_Environment_StepTuff_01,
        Id.Sfx_Environment_StepTuff_02,
        Id.Sfx_Environment_StepTuff_03,
        Id.Sfx_Environment_StepTuff_04,
        Id.Sfx_Environment_StepTuff_05,
        Id.Sfx_Environment_StepTuff_06,
        Id.Sfx_Environment_StepTuff_07,
        Id.Sfx_Environment_StepTuff_08,
        Id.Sfx_Environment_StepTuff_09,
        Id.Sfx_Environment_StepTuff_10,
        Id.Sfx_Environment_StepTuff_11,
    };
    // 타운 ─ 해변 모래
    private static string[] _sandStepArr =
    {
        Id.Sfx_Environment_StepSand_1,
        Id.Sfx_Environment_StepSand_2,
        Id.Sfx_Environment_StepSand_3,
        Id.Sfx_Environment_StepSand_4,
        Id.Sfx_Environment_StepSand_5,
    };
    
    /// <summary>
    /// 카메라에서 발자국 소리를 재생합니다.
    /// </summary>
    public static void TryPlayStepSound(ref float nextStepTime, float stepInterval, Vector2 pos)
    {
        if (UMath.TryCooldownEnd(Time.time, ref nextStepTime, stepInterval))
        {
            var tile = TileManager.Ins.Tile;
            if (tile.IsCaveFloor(pos))
            {
                int index = Random.Range(0, _stoneStepArr.Length);
                USound.PlaySfx(_stoneStepArr[index]);
                return;
            }
            if (tile.IsCaveRail(pos))
            {
                int index = Random.Range(0, _woodStepArr.Length);
                USound.PlaySfx(_woodStepArr[index]);
                return;
            }
            if (tile.IsForestGrass(pos))
            {
                int index = Random.Range(0, _wetGrassStepArr.Length);
                USound.PlaySfx(_wetGrassStepArr[index]);
                return;
            }
            if (tile.IsWoodBridge(pos))
            {
                int index = Random.Range(0, _ladderStepArr.Length);
                USound.PlaySfx(_ladderStepArr[index]);
                return;
            }
            if (tile.IsTownStoneRoad(pos))
            {
                int index = Random.Range(0, _scaffoldStepArr.Length);
                USound.PlaySfx(_scaffoldStepArr[index]);
                return;
            }
            if (tile.IsTownGrass(pos))
            {
                int index = Random.Range(0, _grassStepArr.Length);
                USound.PlaySfx(_grassStepArr[index]);
                return;
            }
            if (tile.IsTownFlowerGrass(pos))
            {
                int index = Random.Range(0, _clothStepArr.Length);
                USound.PlaySfx(_clothStepArr[index]);
                return;
            }
            if (tile.IsTownDirtRoad(pos))
            {
                int index = Random.Range(0, _gravelStepArr.Length);
                USound.PlaySfx(_gravelStepArr[index]);
                return;
            }
            if (tile.IsTownFarmDirt(pos))
            {
                int index = Random.Range(0, _tuffStepArr.Length);
                USound.PlaySfx(_tuffStepArr[index]);
                return;
            }
            if (tile.IsTownSandRoad(pos))
            {
                int index = Random.Range(0, _sandStepArr.Length);
                USound.PlaySfx(_sandStepArr[index]);
                return;
            }
        }
    }
}
