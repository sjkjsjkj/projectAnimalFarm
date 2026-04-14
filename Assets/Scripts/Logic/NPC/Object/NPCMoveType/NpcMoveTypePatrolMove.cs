using System;
using UnityEngine;

/// <summary>
/// 일정 구간을 순찰이동하는 스크립트
/// </summary>
public class NpcMoveTypePatrolMove : NpcMoveTypeBase
{
    private Vector3[] _patrolTarget;
    private int _currentPatrolIndex;

    private float _moveSpeed;

    public event Action OnNextMove;
    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void InitSetting(Vector3[] patrolTargets, float moveSpeed)
    {
        UDebug.Print($"셋팅 확인 Patrol\npatrolTargets : {patrolTargets.Length}\nmoveSpeed : {moveSpeed}");
        _patrolTarget = patrolTargets;
        _moveSpeed = moveSpeed;
    }
    public override void Move()
    {
        if (Vector3.Distance(transform.position, _patrolTarget[_currentPatrolIndex]) <= 0.1f)
        {
            OnNextMove?.Invoke();
            return;
        }

        transform.position += (_patrolTarget[_currentPatrolIndex] - transform.position).normalized * Time.deltaTime * _moveSpeed;
    }

    public override Vector3 NextTargetFind()
    {
        //UDebug.Print("AreaMoveNPC : NextTargetFind");
        _currentPatrolIndex = _currentPatrolIndex == _patrolTarget.Length - 1? 0 : _currentPatrolIndex + 1;

        return _patrolTarget[_currentPatrolIndex];
    }
    #endregion
}

/*int resultDir;
        int diffX, diffY;
        diffX = (int)(Mathf.Abs(transform.position.x - _nextMoveTarget.x));
        diffY = (int)(Mathf.Abs(transform.position.y - _nextMoveTarget.y));
        //diffX = (int)(Mathf.Abs(transform.position.x)- Mathf.Abs(_nextMoveTarget.x));
        //diffY = (int)(Mathf.Abs(transform.position.y) - Mathf.Abs(_nextMoveTarget.y));

        if (Mathf.Abs(diffX) >= Mathf.Abs(diffY))
        {
            resultDir = 1;
            if (_nextMoveTarget.x >= 0.0f)
            {
                resultDir = 0;// _spRenderer.flipX = true;
            }
        }
        else
        {
            resultDir = _nextMoveTarget.y >= 0.0f ? 3 : 2;
        }
        //UDebug.Print($"NextFaceDir : {resultDir}");
        return resultDir;
*/
