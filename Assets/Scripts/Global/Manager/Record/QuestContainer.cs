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
                condition: (data) => data.GetTypeRecord(EType.SeedItem) >= 3f,
                progress: (data) => (data.GetTypeRecord(EType.SeedItem), 3f)
            )
        ),
        new QuestInfo(
            description: string.Format("인벤토리 열어보기 (인벤토리 : E)" +
                "{0}{1}{2}{3}감자 씨앗 5개</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 0,
            rewardItemId: Id.Item_Seed_Potato,
            rewardItemAmount: 5,
            new QuestCondition(
                subtopic: "인벤토리를 연 횟수",
                condition: (data) => data.totalInventoryOpenCount >= 1f,
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
                condition: (data) => data.totalPlowCount >= 5f,
                progress: (data) => (data.totalPlowCount, 5f)
            ),
            new QuestCondition(
                subtopic: "씨앗을 심은 횟수",
                condition: (data) => data.totalPlantingCount >= 5f,
                progress: (data) => (data.totalPlantingCount, 5f)
            ),
            new QuestCondition(
                subtopic: "물을 뿌린 횟수",
                condition: (data) => data.totalWateringCount >= 5f,
                progress: (data) => (data.totalWateringCount, 5f)
            ),
            new QuestCondition(
                subtopic: "작물을 수확한 횟수",
                condition: (data) => data.totalHarvestingCount >= 5f,
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
                condition: (data) => data.totalGatheringCount >= 5,
                progress: (data) => (data.totalGatheringCount, 5f)
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
                condition: (data) => data.totalOreMinedCount >= 5,
                progress: (data) => (data.totalOreMinedCount, 5f)
            )
        ),
        new QuestInfo(
            description: string.Format("마을의 위쪽에서 동물 아이템 구매하기" +
                "{0}{1}{2}{3}닭 2마리</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 0,
            rewardItemId: Id.Item_Animal_Chicken,
            rewardItemAmount: 2,
            new QuestCondition(
                subtopic: "구매한 동물 수",
                condition: (data) => data.GetTypeRecord(EType.AnimalItem) >= 1,
                progress: (data) => (data.GetTypeRecord(EType.AnimalItem), 1f)
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
                condition: (data) => data.GetTypeRecord(EType.AnimalWorld) >= 3,
                progress: (data) => (data.GetTypeRecord(EType.AnimalWorld), 3f)
            )
        ),
        new QuestInfo(
            description: string.Format("마을이나 숲의 낚시 포인트에서 물고기 낚기" +
                "{0}{1}{2}{3}아이스크림 5개</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 0,
            rewardItemId: Id.Item_Consume_IceCream,
            rewardItemAmount: 5,
            new QuestCondition(
                subtopic: "낚은 물고기 수",
                condition: (data) => data.totalFishCaughtCount >= 5,
                progress: (data) => (data.totalFishCaughtCount, 5f)
            )
        ),
        new QuestInfo(
            description: string.Format("동물에게서 생산품 얻기" +
                "{0}{1}{2}{3}500 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 500,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "획득한 생산품 수",
                condition: (data) => data.GetTypeRecord(EType.ProductItem) >= 5,
                progress: (data) => (data.GetTypeRecord(EType.ProductItem), 5f)
            )
        ),
        new QuestInfo(
            description: string.Format("목장에 있는 먹이통 열어보기" +
                "{0}{1}{2}{3}100 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 100,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "먹이통을 연 횟수",
                condition: (data) => data.totalFeedBoxOpenCount >= 1,
                progress: (data) => (data.totalFeedBoxOpenCount, 1f)
            )
        ),
        new QuestInfo(
            description: string.Format("목장 옆에 있는 창고 열어보기" +
                "{0}{1}{2}{3}100 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 100,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "창고를 연 횟수",
                condition: (data) => data.totalChestOpenCount >= 1,
                progress: (data) => (data.totalChestOpenCount, 1f)
            )
        ),
        new QuestInfo(
            description: string.Format("마을 상단의 제작대 열어보기" +
                "{0}{1}{2}{3}100 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 100,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "제작대를 연 횟수",
                condition: (data) => data.totalCraftingOpenCount >= 1,
                progress: (data) => (data.totalCraftingOpenCount, 1f)
            )
        ),
        new QuestInfo(
            description: string.Format("중급 도구 제작하기" +
                "{0}{1}{2}{3}3000 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 3000,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "중급 삽",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_SolidShovel) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_SolidShovel), 1f)
            ),
            new QuestCondition(
                subtopic: "중급 물뿌리개",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_SolidWateringCan) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_SolidWateringCan), 1f)
            ),
            new QuestCondition(
                subtopic: "중급 낫",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_SolidSickle) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_SolidSickle), 1f)
            ),
            new QuestCondition(
                subtopic: "중급 곡괭이",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_SolidPickAxe) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_SolidPickAxe), 1f)
            ),
            new QuestCondition(
                subtopic: "중급 낚싯대",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_SolidFishingRod) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_SolidFishingRod), 1f)
            )
        ),
        new QuestInfo(
            description: string.Format("걷기 운동" +
                "{0}{1}{2}{3}애플 파이 10개</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 0,
            rewardItemId: Id.Item_Consume_ApplePie,
            rewardItemAmount: 10,
            new QuestCondition(
                subtopic: "이동량",
                condition: (data) =>
                    data.totalWalkingDistance >= 300f,
                progress: (data) =>
                    (data.totalWalkingDistance, 300f)
            )
        ),
        new QuestInfo(
            description: string.Format("신비한 물고기" +
                "{0}{1}{2}{3}붕어 100마리</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 0,
            rewardItemId: Id.All_Fish_0,
            rewardItemAmount: 100,
            new QuestCondition(
                subtopic: "눈볼개복",
                condition: (data) => data.GetItemRecord(Id.All_Fish_12) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.All_Fish_12), 1f)
            ),
            new QuestCondition(
                subtopic: "자바리",
                condition: (data) => data.GetItemRecord(Id.All_Fish_38) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.All_Fish_38), 1f)
            ),
            new QuestCondition(
                subtopic: "도화새우",
                condition: (data) => data.GetItemRecord(Id.All_Fish_55) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.All_Fish_55), 1f)
            ),
            new QuestCondition(
                subtopic: "줄판비늘뱀",
                condition: (data) => data.GetItemRecord(Id.All_Fish_82) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.All_Fish_82), 1f)
            )
        ),
        new QuestInfo(
            description: string.Format("고급 도구 제작하기" +
                "{0}{1}{2}{3}10000 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 10000,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "고급 삽",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_SuperShovel) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_SuperShovel), 1f)
            ),
            new QuestCondition(
                subtopic: "고급 물뿌리개",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_SuperWateringCan) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_SuperWateringCan), 1f)
            ),
            new QuestCondition(
                subtopic: "고급 낫",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_SuperSickle) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_SuperSickle), 1f)
            ),
            new QuestCondition(
                subtopic: "고급 곡괭이",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_SuperPickAxe) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_SuperPickAxe), 1f)
            ),
            new QuestCondition(
                subtopic: "고급 낚싯대",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_SuperFishingRod) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_SuperFishingRod), 1f)
            )
        ),
        new QuestInfo(
            description: string.Format("마라톤 러너" +
                "{0}{1}{2}{3}10000 골드, 워터멜론 10개</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 10000,
            rewardItemId: Id.Item_Consume_Watermelon,
            rewardItemAmount: 10,
            new QuestCondition(
                subtopic: "이동량",
                condition: (data) =>
                    data.totalRunningDistance >= 1500f,
                progress: (data) =>
                    (data.totalRunningDistance, 1500f)
            )
        ),
        new QuestInfo(
            description: string.Format("최고급 도구 제작하기" +
                "{0}{1}{2}{3}50000 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 50000,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "최고급 삽",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_PrimeShovel) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_PrimeShovel), 1f)
            ),
            new QuestCondition(
                subtopic: "최고급 물뿌리개",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_PrimeWateringCan) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_PrimeWateringCan), 1f)
            ),
            new QuestCondition(
                subtopic: "최고급 낫",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_PrimeSickle) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_PrimeSickle), 1f)
            ),
            new QuestCondition(
                subtopic: "최고급 곡괭이",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_PrimePickAxe) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_PrimePickAxe), 1f)
            ),
            new QuestCondition(
                subtopic: "최고급 낚싯대",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_PrimeFishingRod) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_PrimeFishingRod), 1f)
            )
        ),
        new QuestInfo(
            description: string.Format("마스터 도구 제작하기" +
                "{0}{1}{2}{3}100000 골드</size></color>", Environment.NewLine, REWARD_COLOR, REWARD_SIZE, REWARD_TEXT),
            rewardGold: 100000,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "마스터 삽",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_MasterShovel) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_MasterShovel), 1f)
            ),
            new QuestCondition(
                subtopic: "마스터 물뿌리개",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_MasterWateringCan) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_MasterWateringCan), 1f)
            ),
            new QuestCondition(
                subtopic: "마스터 낫",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_MasterSickle) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_MasterSickle), 1f)
            ),
            new QuestCondition(
                subtopic: "마스터 곡괭이",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_MasterPickAxe) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_MasterPickAxe), 1f)
            ),
            new QuestCondition(
                subtopic: "마스터 낚싯대",
                condition: (data) => data.GetItemRecord(Id.Item_Tool_MasterFishingRod) >= 1f,
                progress: (data) => (data.GetItemRecord(Id.Item_Tool_MasterFishingRod), 1f)
            )
        ),
    };
}
