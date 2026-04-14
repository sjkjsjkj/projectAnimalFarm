/// <summary>
/// 퀘스트 정보
/// </summary>
public class QuestInfo
{
    public string Description { get; private set; }
    public int RewardGold { get; private set; }
    public string RewardItemId { get; private set; }
    public int RewardItemAmount { get; private set; }
    public QuestCondition[] Condition { get; private set; } // 클래스만 넘기면 알아서 검사

    // 생성자
    public QuestInfo
        (string description, int rewardGold, string rewardItemId, int rewardItemAmount, params QuestCondition[] condition)
    {
        Description = description;
        RewardGold = rewardGold;
        RewardItemId = rewardItemId;
        RewardItemAmount = rewardItemAmount;
        Condition = condition;
    }

    /// <summary>
    /// 현재 목표의 모든 조건을 달성했는지 검사합니다.
    /// </summary>
    public bool IsComplete(RecordData data)
    {
        for (int i = 0; i < Condition.Length; ++i)
        {
            if (!Condition[i].IsClear(data))
            {
                return false;
            }
        }
        return true;
    }
}
