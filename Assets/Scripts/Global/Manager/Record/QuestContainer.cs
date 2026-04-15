using System.Collections.Generic;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public static class QuestContainer
{
    public static readonly IReadOnlyList<QuestInfo> list = new List<QuestInfo>
    {
        new QuestInfo(
            description: "마을 걸어보기 (이동 : WASD)",
            rewardGold: 500,
            rewardItemId: Id.Item_Tool_BasicShovel,
            rewardItemAmount: 1,
            new QuestCondition(
                subtopic: "이동량",
                condition: (data) =>
                    data.totalWalkingDistance >= 10f,
                progress: (data) =>
                    (data.totalWalkingDistance, 10f)
            )
        ),
        new QuestInfo(
            description: "마을 달려보기 (달리기 : Shift)",
            rewardGold: 500,
            rewardItemId: Id.Item_Tool_BasicWateringCan,
            rewardItemAmount: 1,
            new QuestCondition(
                subtopic: "이동량",
                condition: (data) =>
                    data.totalRunningDistance >= 30f,
                progress: (data) =>
                    (data.totalRunningDistance, 30f)
            )
        ),
        new QuestInfo(
            description: "마을에서 NPC에게 씨앗을 구매해보세요. (상호작용 : F)",
            rewardGold: 500,
            rewardItemId: Id.Item_Tool_BasicSickle,
            rewardItemAmount: 1,
            new QuestCondition(
                subtopic: "구매한 씨앗",
                condition: (data) => data.GetTypeRecord(EType.SeedItem) >= 1,
                progress: (data) => (data.GetTypeRecord(EType.SeedItem), 1f)
            )
        ),
        new QuestInfo(
            description: "인벤토리 열어보기 (인벤토리 : I)",
            rewardGold: 100,
            rewardItemId: Id.Item_Seed_BlueBerry,
            rewardItemAmount: 4,
            new QuestCondition(
                subtopic: "인벤토리를 연 횟수",
                condition: (data) => data.totalInventoryOpenCount >= 1,
                progress: (data) => (data.totalInventoryOpenCount, 1f)
            )
        ),
        new QuestInfo(
            description: "마을 왼측의 농장에서 농사해보기",
            rewardGold: 300,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "밭을 갈은 횟수",
                condition: (data) => data.totalPlowCount >= 5,
                progress: (data) => (data.totalPlowCount, 5f)
            ),
            new QuestCondition(
                subtopic: "씨앗을 심은 횟수",
                condition: (data) => data.totalPlantingCount >= 5,
                progress: (data) => (data.totalPlantingCount, 5f)
            ),
            new QuestCondition(
                subtopic: "물을 뿌린 횟수",
                condition: (data) => data.totalWateringCount >= 5,
                progress: (data) => (data.totalWateringCount, 5f)
            ),
            new QuestCondition(
                subtopic: "작물을 수확한 횟수",
                condition: (data) => data.totalHarvestingCount >= 5,
                progress: (data) => (data.totalHarvestingCount, 5f)
            )
        ),
        new QuestInfo(
            description: "도감 열어보기 (도감 : O)",
            rewardGold: 100,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "도감을 연 횟수",
                condition: (data) => data.totalPictorialOpenCount >= 1,
                progress: (data) => (data.totalPictorialOpenCount, 1f)
            )
        ),
        new QuestInfo(
            description: "채집가",
            rewardGold: 200,
            rewardItemId: Id.Item_Tool_BasicPickAxe,
            rewardItemAmount: 1,
            new QuestCondition(
                subtopic: "채집 횟수",
                condition: (data) => data.totalGatheringCount >= 5,
                progress: (data) => (data.totalGatheringCount, 5f)
            )
        ),
        new QuestInfo(
            description: "첫 광산",
            rewardGold: 400,
            rewardItemId: Id.Item_Tool_BasicFishingRod,
            rewardItemAmount: 1,
            new QuestCondition(
                subtopic: "채광 횟수",
                condition: (data) => data.totalOreMinedCount >= 10,
                progress: (data) => (data.totalOreMinedCount, 10f)
            )
        ),
        new QuestInfo(
            description: "목장 준비",
            rewardGold: 500,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "구매한 동물 수",
                condition: (data) => data.GetTypeRecord(EType.AnimalWorld) >= 1,
                progress: (data) => (data.GetTypeRecord(EType.AnimalWorld), 1f)
            )
        ),
        new QuestInfo(
            description: "목장 주인",
            rewardGold: 500,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "획득한 생산품 수",
                condition: (data) => data.GetTypeRecord(EType.ProductItem) >= 3,
                progress: (data) => (data.GetTypeRecord(EType.ProductItem), 3f)
            )
        ),
        new QuestInfo(
            description: "낚시꾼",
            rewardGold: 400,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "낚시 횟수",
                condition: (data) => data.totalFishCaughtCount >= 5,
                progress: (data) => (data.totalFishCaughtCount, 5f)
            )
        )
    };
}
