using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "NpcMoveTypeAreaSO_", menuName = "ScriptableObjects/World/NPCMoveType/Area", order = 1)]

public class NpcWorldAreaMoveSO : NpcMoveTypeSO //:NpnWorldSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("이동 범위 (초기 위치로부터 최소,최대 몇까지 움직일 수 있는지)")]
    [SerializeField] protected Vector2 _minPos;     //최초 0,0 에서 스폰되었다면 X는 minPos.X ~ maxPos.X 까지 이동 가능.
    [SerializeField] protected Vector2 _maxPos;     //최조 4,34에서 스폰 되었다면 X는 4 + (minPos X ~ manxPos.X) // Y는 34 + (minPosY ~ maxPos.Y)까지 이동 가능하다.
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Vector2 MinPos => _minPos;
    public Vector2 MaxPos => _maxPos;

    // 값 유효성 검사
    //public override bool IsValid()
    public bool IsValid()
    {
       // base.IsValid();
        if (_minPos == Vector2.zero || _maxPos == Vector2.zero) return false;
        return true;
    }
    #endregion


    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    //protected override void OnValidate()
    protected void OnValidate()
    {
        //base.OnValidate();

        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (Name = {this.name}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
