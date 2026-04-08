using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "NpcWorldSO_", menuName = "ScriptableObjects/World/NPC/PatrolMoveNpc", order = 1)]

public class NpcWorldPatrolMoveSo : NpcWorldSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("이동 경로 (0번째 값은 첫 위치의 값)")]
    [SerializeField] protected Vector2[] _patrolPoints;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Vector2[] PatrolPoints => _patrolPoints;
    
    // 값 유효성 검사
    public override bool IsValid()
    {
        base.IsValid();
        if (_patrolPoints == null || _patrolPoints.Length == 0) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void OnValidate()
    {
        base.OnValidate();

        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {_id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
