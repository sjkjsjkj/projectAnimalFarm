using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 퀘스트 Ui 텍스트를 매 프레임 업데이트합니다.
/// </summary>
public class QuestUiUpdator : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("주제")]
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _condition;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly StringBuilder _sb = new(256);
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        RefreshUi();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void RefreshUi()
    {
        var record = DataManager.Ins.Record;
        // 모든 퀘스트 클리어
        if (record.goalIndex >= QuestContainer.list.Count)
        {
            _title.text = "게임을 플레이해주셔서 감사드립니다!";
            _condition.text = null;
            return;
        }
        // 현재 퀘스트
        QuestManager.Ins.CheckQuestProgress();
        var curQuest = QuestContainer.list[record.goalIndex];
        // 메시지 작성
        _sb.Clear();
        int length = curQuest.Condition.Length;
        for (int i = 0; i < length; ++i)
        {
            var cond = curQuest.Condition[i];
            string subtitle = cond.GetSubtopic();
            (float cur, float need) = cond.GetProgress(record);
            // 이동 퀘스트
            if (record.goalIndex <= 1)
            {
                _sb.AppendLine($"    {subtitle} : {cur:F0}m / {need:F0}m");
            }
            else
            {
                _sb.AppendLine($"    {subtitle} : {cur:F0} / {need:F0}");
            }
        }
        _condition.SetText(_sb);
    }

    private void QuestChangedHandle(OnQuestChanged data)
    {
        var record = DataManager.Ins.Record;
        int count = QuestContainer.list.Count;
        if (record.goalIndex >= count)
        {
            _title.text = "게임을 플레이해주셔서 감사드립니다!";
            _condition.text = null;
            return;
        }
        var curQuest = QuestContainer.list[record.goalIndex];
        _title.text = $"({record.goalIndex + 1}/{count + 1})  {curQuest.Description}";
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus<OnQuestChanged>.Subscribe(QuestChangedHandle);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus<OnQuestChanged>.Unsubscribe(QuestChangedHandle);
    }
    private void Start()
    {
        QuestChangedHandle(new OnQuestChanged(DataManager.Ins.Record.goalIndex, null));
    }
    #endregion
}
