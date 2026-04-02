using UnityEngine;

/// <summary>
/// 일정구역을 배회하는? NPC의 스크립트
/// </summary>
public class NpcMoveTypeAreaMove : NpcMoveTypeBase
{
    private int _minX, _minY, _maxX, _maxY;
    private Vector2 _pos;
    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Move()
    {
        RandomDirSetting();
    }
    private Vector3 RandomDirSetting()
    {
        float dirX, dirY;
        int resultDir;  // 1 : (동)서 / 2 : 남 / 3 : 북

        dirX = Random.Range(_pos.x > _minX ? -1 : 0, _pos.x < _maxX ? 0 : 1);
        dirY = Random.Range(_pos.y > _minY ? -1 : 0, _pos.y < _maxY ? 0 : 1);

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
