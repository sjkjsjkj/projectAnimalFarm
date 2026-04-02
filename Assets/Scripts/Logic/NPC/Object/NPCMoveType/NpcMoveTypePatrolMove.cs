using System.Collections;
using UnityEngine;

/// <summary>
/// 일정 구간을 순찰이동하는 스크립트
/// </summary>
public class NpcMoveTypePatrolMove : NpcMoveTypeBase
{
    private Vector3[] _patrolTarget;
    private int _currentPatrolIndex;
    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Move()
    {
        NextTarget();
        UDebug.Print($"현재 타겟 : {_currentPatrolIndex} ");
    }
    private void NextTarget()
    {
        _currentPatrolIndex = _currentPatrolIndex -1 == _patrolTarget.Length?0:_currentPatrolIndex+1;
    }

    private IEnumerator PatrolCoroutine()
    {
        while(true)
        {
            
            yield return null;

            float distance = Vector3.Distance(transform.position , _patrolTarget[0]);

            if(distance <= 0.1f)
            {
                break;
            }
            //Todo : npc의 위치와 patrolTarget 의 위치가 비슷하다면 
        }
        
    }
    #endregion
}
