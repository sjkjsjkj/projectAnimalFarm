/// <summary>
/// NPC의 각 상태가 가질 부모 클래스
/// </summary>
public abstract class NPCState
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private NPCFSM _npcFSM;
    #endregion

    #region ─────────────────────────▶  생성자  ◀─────────────────────────
    public NPCState(NPCFSM npcFSM)
    {
        _npcFSM = npcFSM;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public abstract void StateEnter();

    public abstract void StateExit();

    public abstract void StateUpdate();

    #endregion
}
