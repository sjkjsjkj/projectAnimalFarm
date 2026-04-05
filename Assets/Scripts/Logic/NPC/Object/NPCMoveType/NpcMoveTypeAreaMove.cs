using UnityEngine;

/// <summary>
/// 일정구역을 배회하는? NPC의 스크립트
/// </summary>
public class NpcMoveTypeAreaMove : NpcMoveTypeBase
{
    //private float _minX, _minY, _maxX, _maxY;
    private Vector2 _minPos, _maxPos;
  
    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Move()
    {
        RandomDirSetting();
    }
    public void AreaRangeSetting(Vector2 initPos, Vector2 minPos, Vector2 maxPos)
    {
        _minPos = new Vector2(minPos.x + initPos.x, minPos.y + initPos.y);
        _maxPos = new Vector2(maxPos.x + initPos.x, maxPos.y + initPos.y);
    }
    private Vector3 RandomDirSetting()
    {
        float dirX, dirY;
        int resultDir;  // 1 : (동)서 / 2 : 남 / 3 : 북

        dirX = Random.Range(transform.localPosition.x > _minPos.x ? -1 : 0, transform.localPosition.x < _maxPos.x ? 0 : 1);
        dirY = Random.Range(transform.localPosition.y > _minPos.y ? -1 : 0, transform.localPosition.y < _maxPos.y ? 0 : 1);

        if (Mathf.Abs(dirX) >= Mathf.Abs(dirY))
        {
            resultDir = 1;
            if (dirX >= 0.0f)
            {
                _npc.SpRenderer.flipX = true;
            }
            else
            {
                _npc.SpRenderer.flipX = false;
            }
        }
        else
        {
            resultDir = dirY >= 0.0f ? 3 : 2;
        }
        _npc.Animator.SetInteger("FaceDir", resultDir);

        return new Vector3(dirX, dirY).normalized;
    }
    #endregion
}
