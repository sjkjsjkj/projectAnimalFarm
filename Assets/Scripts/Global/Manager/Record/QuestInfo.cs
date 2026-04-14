using System;

/// <summary>
/// 퀘스트 정보
/// </summary>
public class QuestInfo
{
    public string Description { get; private set; }
    public int RewardGold { get; private set; }
    public string RewardItemId { get; private set; }
    public int RewardItemAmount { get; private set; }
    public Predicate<RecordData> Condition { get; private set; } // 클래스만 넘기면 알아서 검사

    // 생성자
    public QuestInfo
        (string description, int rewardGold, string rewardItemId, int rewardItemAmount, Predicate<RecordData> condition)
    {
        Description = description;
        RewardGold = rewardGold;
        RewardItemId = rewardItemId;
        RewardItemAmount = rewardItemAmount;
        Condition = condition;
    }
}
