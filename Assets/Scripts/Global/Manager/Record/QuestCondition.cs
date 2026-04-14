using System;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class QuestCondition
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Func<RecordData, bool> _condition; // 조건식
    private Func<RecordData, (float cur, float need)> _progress;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion
}
