using System;
using System.Collections.Generic;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public static class QuestContainer
{
    private const string REWARD_TEXT = "보상 : ";
    private const string REWARD_COLOR = "<color=#FFD700>";
    private const string REWARD_SIZE = "<size=80%>";

    public static readonly IReadOnlyList<QuestInfo> list = new List<QuestInfo>
    {
        new QuestInfo(
            description: string.Format("마을 걸어보기 (이동 : WASD)" +
                "{0}{1}{2}{3}기본 삽, 100 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 100,
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
            description: string.Format("마을 달려보기 (달리기 : Shift)" +
                "{0}{1}{2}{3}기본 물뿌리개, 100 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 100,
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
            description: string.Format("마을 우측 하단의 NPC에게 씨앗 구매하기 (상호작용 : F)" +
                "{0}{1}{2}{3}기본 낫, 100 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 100,
            rewardItemId: Id.Item_Tool_BasicSickle,
            rewardItemAmount: 1,
            new QuestCondition(
                subtopic: "구매한 씨앗",
                condition: (data) => data.GetTypeRecord(EType.SeedItem) >= 3,
                progress: (data) => (data.GetTypeRecord(EType.SeedItem), 3f)
            )
        ),
        new QuestInfo(
            description: string.Format("인벤토리 열어보기 (인벤토리 : I)" +
                "{0}{1}{2}{3}감자 씨앗 5개</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 0,
            rewardItemId: Id.Item_Seed_Potato,
            rewardItemAmount: 5,
            new QuestCondition(
                subtopic: "인벤토리를 연 횟수",
                condition: (data) => data.totalInventoryOpenCount >= 1,
                progress: (data) => (data.totalInventoryOpenCount, 1f)
            )
        ),
        new QuestInfo(
            description: string.Format("마을 왼측의 농장에서 농사하기" +
                "{0}{1}{2}{3}300 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
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
            description: string.Format("도감 열어보기 (도감 : O)" +
                "{0}{1}{2}{3}100 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
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
            description: string.Format("마을 우측의 숲에서 채집하기 (채집 : F)" +
                "{0}{1}{2}{3}기본 곡괭이</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 0,
            rewardItemId: Id.Item_Tool_BasicPickAxe,
            rewardItemAmount: 1,
            new QuestCondition(
                subtopic: "채집 횟수",
                condition: (data) => data.totalGatheringCount >= 10,
                progress: (data) => (data.totalGatheringCount, 10f)
            )
        ),
        
        new QuestInfo(
            description: string.Format("숲 안쪽의 광산에서 광물 캐기 (채광 : F)" +
                "{0}{1}{2}{3}기본 낚싯대</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 0,
            rewardItemId: Id.Item_Tool_BasicFishingRod,
            rewardItemAmount: 1,
            new QuestCondition(
                subtopic: "채광 횟수",
                condition: (data) => data.totalOreMinedCount >= 10,
                progress: (data) => (data.totalOreMinedCount, 10f)
            )
        ),
        new QuestInfo(
            description: string.Format("마을의 위쪽에서 동물 아이템 구매하기" +
                "{0}{1}{2}{3}1000 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 1000,
            rewardItemId: Id.World_Animal_Chicken,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "들여온 동물 수",
                condition: (data) => data.GetTypeRecord(EType.AnimalWorld) >= 1,
                progress: (data) => (data.GetTypeRecord(EType.AnimalWorld), 1f)
            )
        ),
        new QuestInfo(
            description: string.Format("목장에 동물 들여오기" +
                "{0}{1}{2}{3}1000 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 1000,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "들여온 동물 수",
                condition: (data) => data.GetTypeRecord(EType.AnimalWorld) >= 1,
                progress: (data) => (data.GetTypeRecord(EType.AnimalWorld), 1f)
            )
        ),
        new QuestInfo(
            description: "마을이나 숲의 낚시 포인트에서 물고기 낚기",
            rewardGold: 200,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "낚시 횟수",
                condition: (data) => data.totalFishCaughtCount >= 5,
                progress: (data) => (data.totalFishCaughtCount, 5f)
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
    };
}
