using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "NpcMoveTypePatrolSO_", menuName = "ScriptableObjects/World/NPCMoveType/Patrol", order = 1)]

public class NpcWorldPatrolMoveSo : NpcMoveTypeSO//NpcWorldSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("이동 경로 (0번째 값은 첫 위치의 값)")]
    [SerializeField] protected Vector3[] _patrolPoints;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Vector3[] PatrolPoints => _patrolPoints;

    // 값 유효성 검사
    //public override bool IsValid()
    public bool IsValid()
    {
        //base.IsValid();
        if (_patrolPoints == null || _patrolPoints.Length == 0) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    //protected override void OnValidate()
    protected void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {this.name}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
