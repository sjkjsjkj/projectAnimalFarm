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
            rewardGold: 100,
            rewardItemId: null,
            rewardItemAmount: 0,
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
            rewardGold: 100,
            rewardItemId: null,
            rewardItemAmount: 0,
            new QuestCondition(
                subtopic: "이동량",
                condition: (data) =>
                    data.totalRunningDistance >= 10f,
                progress: (data) =>
                    (data.totalRunningDistance, 10f)
            )
        ),
    };
}
