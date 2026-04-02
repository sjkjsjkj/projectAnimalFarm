/// <summary>
/// NPC가 움직이고 있는 상태
/// </summary>
public class NPCStateMove : NPCState
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public NPCStateMove(NPCFSM npcFSM) : base(npcFSM) { }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void StateEnter()
    {
        throw new System.NotImplementedException();
    }
    public override void StateExit()
    {
        throw new System.NotImplementedException();
    }
    public override void StateUpdate()
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
