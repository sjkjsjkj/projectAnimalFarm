using System;

/// <summary>
/// 목표의 각 조건을 정의하는 구조체
/// </summary>
public readonly struct QuestCondition
{
    private readonly string _subtopic;
    private readonly Func<RecordData, bool> _condition; // 조건식
    private readonly Func<RecordData, (float cur, float need)> _progress;

    public QuestCondition(string subtopic,
        Func<RecordData, bool> condition, Func<RecordData, (float cur, float need)> progress)
    {
        _subtopic = subtopic;
        _condition = condition;
        _progress = progress;
    }

    public string GetSubtopic() => _subtopic;

    public bool IsClear(RecordData data)
    {
        return _condition.Invoke(data);
    }

    public (float cur, float need) GetProgress(RecordData data)
    {
        if(_progress == null)
        {
            UDebug.Print($"{_subtopic}에서 Progress Func가 할당되지 않았습니다.");
            return (0f, 1f);
        }
        var progress = _progress.Invoke(data);
        float cur = MathF.Min(progress.cur, progress.need);
        return (cur, progress.need);
    }
}
