/// <summary>
/// 안움직이는 NPC 스크립트
/// </summary>
public class NpcMoveTypeDontMove : NpcMoveTypeBase
{

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Move()
    {
        //UDebug.Print($"이 객체는 움직이지 않습니다.");
    }
    public override int NextTargetFind()
    {
        return 2;
    }
    #endregion
}
