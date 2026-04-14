using System;
using UnityEngine;

/// <summary>
/// 일정구역을 배회하는? NPC의 스크립트
/// </summary>
public class NpcMoveTypeAreaMove : NpcMoveTypeBase
{
    //private float _minX, _minY, _maxX, _maxY;
    [SerializeField] private Vector2 _minPos, _maxPos;
    [SerializeField] private Vector3 _nextMoveTarget;

    private float _moveSpeed;

    public event Action OnNextMove;

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void InitSetting(Vector2 initPos, Vector2 minPos, Vector2 maxPos, float moveSpeed)
    {
        UDebug.Print("셋팅 확인 Area");
        _minPos = new Vector2(minPos.x + initPos.x, minPos.y + initPos.y);
        _maxPos = new Vector2(maxPos.x + initPos.x, maxPos.y + initPos.y);
        _moveSpeed = moveSpeed;
    }


    public override void Move()
    {
        if (Vector3.Distance(transform.position, _nextMoveTarget) <= 0.1f)
        {
            OnNextMove?.Invoke();
            return;
        }

        transform.position += (_nextMoveTarget - transform.position).normalized*Time.deltaTime* _moveSpeed;
    }
    
    public override Vector3 NextTargetFind()
    {
        //UDebug.Print("AreaMoveNPC : NextTargetFind");
        _nextMoveTarget = new Vector3(UnityEngine.Random.Range(_minPos.x, _maxPos.x), UnityEngine.Random.Range(_minPos.y, _maxPos.y));

        return _nextMoveTarget;
        
    }
    #endregion
}
