/// <summary>
/// 데이터 타입을 정의하는 열거형
/// </summary>
public enum EType
{
    None = 0,
    #region ─────────────────────────▶ 아이템 ◀─────────────────────────
    // 도구
    HoeItem = 1, // 괭이
    AxeItem = 18, // 도끼
    PickaxeItem = 19, // 곡괭이
    WateringCan = 20, // 물뿌리개
    // 소모품
    BaitItem = 2, // 미끼
    SeedItem = 3, // 씨앗
    FeedItem = 4, // 먹이
    SpecialItem = 21, // 특수 아이템
    // 재료
    HarvestItem = 5, // 수확물
    WoodItem = 6, // 나무
    FishItem = 7, // 생선
    OreItem = 8, // 광물
    #endregion

    #region ─────────────────────────▶ 월드 ◀─────────────────────────
    PlayerWorld = 9,
    NpcWorld = 10,
    // 농업
    AnimalWorld = 11, // 동물
    CropWorld = 12, // 작물
    // 물고기
    FishWorld = 13, // 사용할 예정 없음
    // 자연 생성물
    TreeWorld = 14, // 나무
    GatherWorld = 15, // 채집
    OreWorld = 16, // 광물
    #endregion

    #region ─────────────────────────▶ 기타 ◀─────────────────────────
    Recipe = 17,
    #endregion
}
