using UnityEngine;

/// <summary>
/// NPC의 이동 타입의 부모가 되는 클래스
/// </summary>
public abstract class NpcMoveTypeBase : BaseMono
{
    protected NPCObject _npc;
    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public abstract void Move();
    public void SetNpc(NPCObject npc)
    {
        _npc = npc;
    }
    #endregion
}
