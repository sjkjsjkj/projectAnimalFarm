/// <summary>
/// NPC의 기본 상태
/// </summary>
public class NPCStateIdle : NPCState
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private float _timer = 0;
    private float _tickTime = 2f;
    #endregion

    #region ─────────────────────────▶ 생성자 ◀─────────────────────────
    public NPCStateIdle(NPCFSM nPCFSM) : base(nPCFSM) { }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void StateEnter()
    {
        _timer = 0;
    }
    public override void StateExit()
    {
        throw new System.NotImplementedException();
    }
    public override void StateUpdate()
    {
        _timer += UnityEngine.Time.deltaTime;

        if (_timer > _tickTime)
        {

        }
    }
    #endregion
}
