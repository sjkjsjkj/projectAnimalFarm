/// <summary>
/// NPC의 상태를 관리하는 스크립트 입니다.
/// </summary>
public class NPCFSM
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private NPCState _state;

    private NPCObject _npcMaster;
    private NPCMoveController _npcMover;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion
    #region ─────────────────────────▶  생성자  ◀─────────────────────────
    public NPCFSM(NPCObject npcMaster, NPCMoveController npcMover)
    {
        _npcMaster = npcMaster;
        _npcMover = npcMover;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void SetState(NPCState state)
    {
        state.StateExit();

        this._state = state;

        state.StateEnter();
    }
    public void Tick()
    {

    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion
}
