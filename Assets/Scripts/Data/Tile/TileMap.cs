using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// 2D 격자 맵의 타일 상태를 관리하는 클래스입니다.
/// </summary>
public class TileMap
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly TileSingle[] _tiles;
    private readonly int _width;
    private readonly int _height;
    private readonly int _startX;
    private readonly int _startY;
    #endregion

    #region ─────────────────────────▶ 프로퍼티 ◀─────────────────────────
    /// <summary>
    /// 맵의 가로 길이(셀 개수)입니다.
    /// </summary>
    public int Width => _width;

    /// <summary>
    /// 맵의 세로 길이(셀 개수)입니다.
    /// </summary>
    public int Height => _height;

    /// <summary>
    /// 맵의 가로 길이 * 세로 길이입니다.
    /// </summary>
    public int Length => _tiles.Length;
    #endregion

    #region ─────────────────────────▶ 상태 접근 (편의성) ◀─────────────────────────

    /// <summary>
    /// 디버깅 용도 : 타일 원본 ID 반환
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetTileID(Vector2 worldPos) => GetTileID(WorldToIndex(worldPos));

    /// <summary>
    /// 디버깅 용도 : 타일 원본 ID 반환
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetTileID(Vector2Int gridPos) => GetTileID(GridToIndex(gridPos));

    /// <summary>
    /// 상태가 존재하지 않는 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNone(Vector2 worldPos) => IsNone(WorldToIndex(worldPos));

    /// <summary>
    /// 상태가 존재하지 않는 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNone(Vector2Int gridPos) => IsNone(GridToIndex(gridPos));

    /// <summary>
    /// 걸어다닐 수 있는 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveable(Vector2 worldPos) => IsMoveable(WorldToIndex(worldPos));

    /// <summary>
    /// 걸어다닐 수 있는 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveable(Vector2Int gridPos) => IsMoveable(GridToIndex(gridPos));

    /// <summary>
    /// 경작이 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFarmable(Vector2 worldPos) => IsFarmable(WorldToIndex(worldPos));

    /// <summary>
    /// 경작이 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFarmable(Vector2Int gridPos) => IsFarmable(GridToIndex(gridPos));

    /// <summary>
    /// 낚시가 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFishingable(Vector2 worldPos) => IsFishingable(WorldToIndex(worldPos));

    /// <summary>
    /// 낚시가 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFishingable(Vector2Int gridPos) => IsFishingable(GridToIndex(gridPos));

    /// <summary>
    /// 위에 건설이 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBuildable(Vector2 worldPos) => IsBuildable(WorldToIndex(worldPos));

    /// <summary>
    /// 위에 건설이 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBuildable(Vector2Int gridPos) => IsBuildable(GridToIndex(gridPos));

    /// <summary>
    /// 상호작용이 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInteractable(Vector2 worldPos) => IsInteractable(WorldToIndex(worldPos));

    /// <summary>
    /// 상호작용이 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInteractable(Vector2Int gridPos) => IsInteractable(GridToIndex(gridPos));

    /// <summary>
    /// 바다 낚시가 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSeaFishingable(Vector2 worldPos) => IsSeaFishingable(WorldToIndex(worldPos));

    /// <summary>
    /// 바다 낚시가 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSeaFishingable(Vector2Int gridPos) => IsSeaFishingable(GridToIndex(gridPos));
    #endregion

    #region ─────────────────────────▶ 상태 접근 ◀─────────────────────────
    /// <summary>
    /// 디버깅 용도 : 타일 원본 ID 반환
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetTileID(int index)
    {
        if (IsValid(index))
        {
            return _tiles[index].id;
        }
        return -1;
    }

    /// <summary>
    /// 상태가 존재하지 않는 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNone(int index)
    {
        if (IsValid(index))
        {
            return _tiles[index].state == 0;
        }
        return true;
    }

    /// <summary>
    /// 걸어다닐 수 있는 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveable(int index)
    {
        if (IsValid(index))
        {
            return (_tiles[index].state & ETileState.Moveable) != 0;
        }
        return false;
    }

    /// <summary>
    /// 경작이 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFarmable(int index)
    {
        if (IsValid(index))
        {
            return (_tiles[index].state & ETileState.Farmable) != 0;
        }
        return false;
    }

    /// <summary>
    /// 낚시가 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFishingable(int index)
    {
        if (IsValid(index))
        {
            return (_tiles[index].state & ETileState.Fishingable) != 0;
        }
        return false;
    }

    /// <summary>
    /// 위에 건설이 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBuildable(int index)
    {
        if (IsValid(index))
        {
            return (_tiles[index].state & ETileState.Buildable) != 0;
        }
        return false;
    }

    /// <summary>
    /// 상호작용이 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInteractable(int index)
    {
        if (IsValid(index))
        {
            return (_tiles[index].state & ETileState.Interactable) != 0;
        }
        return false;
    }

    /// <summary>
    /// 바다 낚시가 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSeaFishingable(int index)
    {
        if (IsValid(index))
        {
            return (_tiles[index].state & ETileState.SeaFishingable) != 0;
        }
        return false;
    }
    #endregion

    #region ─────────────────────────▶ 유틸리티 ◀─────────────────────────
    /// <summary>
    /// 월드 좌표를 격자 좌표로 변환하여 반환합니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int WorldToGrid(Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 월드 좌표를 격자 좌표로 변환하여 반환합니다.
    /// </summary>
    /// <param name="x">월드 X 좌표</param>
    /// <param name="y">월드 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int x, int y) WorldToGrid(float x, float y)
    {
        int nX = Mathf.FloorToInt(x);
        int nY = Mathf.FloorToInt(y);
        return (nX, nY);
    }

    /// <summary>
    /// 격자 좌표를 월드 좌표로 변환하여 중심점을 반환합니다.
    /// </summary>
    /// <param name="pos">격자 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 GridToWorld(Vector2Int pos)
    {
        float x = (float)pos.x + K.GRID_SIZE_HALF;
        float y = (float)pos.y + K.GRID_SIZE_HALF;
        return new Vector2(x, y);
    }

    /// <summary>
    /// 격자 좌표를 월드 좌표로 변환하여 중심점을 반환합니다.
    /// </summary>
    /// <param name="x">격자 X 좌표</param>
    /// <param name="y">격자 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float x, float y) GridToWorld(int x, int y)
    {
        return ((float)x + K.GRID_SIZE_HALF, (float)y + K.GRID_SIZE_HALF);
    }

    /// <summary>
    /// 격자 좌표를 격자 배열에서 사용할 인덱스로 변환합니다.
    /// </summary>
    /// <param name="x">격자 X 좌표</param>
    /// <param name="y">격자 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GridToIndex(int x, int y)
    {
        int localX = x - _startX;
        int localY = y - _startY;
        return (localY * _width) + localX;
    }

    /// <summary>
    /// 격자 좌표를 격자 배열에서 사용할 인덱스로 변환합니다.
    /// </summary>
    /// <param name="x">격자 X 좌표</param>
    /// <param name="y">격자 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GridToIndex(Vector2Int pos)
    {
        return GridToIndex(pos.x, pos.y);
    }

    /// <summary>
    /// 월드 좌표를 격자 배열에서 사용할 인덱스로 변환합니다.
    /// </summary>
    /// <param name="x">월드 X 좌표</param>
    /// <param name="y">월드 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WorldToIndex(float x, float y)
    {
        return GridToIndex(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
    }

    /// <summary>
    /// 월드 좌표를 격자 배열에서 사용할 인덱스로 변환합니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WorldToIndex(Vector2 pos)
    {
        return GridToIndex(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
    }

    /// <summary>
    /// 격자 배열에서 사용하는 인덱스를 격자 좌표로 변환합니다.
    /// </summary>
    /// <param name="index">격자 배열 인덱스</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2Int IndexToGrid(int index)
    {
        int x = _startX + (index % _width);
        int y = _startY + (index / _width);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 격자 배열에서 사용하는 인덱스를 월드 좌표로 변환합니다.
    /// </summary>
    /// <param name="index">격자 배열 인덱스</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 IndexToWorld(int index)
    {
        Vector2Int nPos = IndexToGrid(index);
        float fX = (float)nPos.x + K.GRID_SIZE_HALF;
        float fY = (float)nPos.y + K.GRID_SIZE_HALF;
        return new Vector2(fX, fY);
    }

    /// <summary>
    /// 인덱스가 유효한 격자 배열 인덱스인지 검사합니다.
    /// </summary>
    /// <param name="index">격자 배열 인덱스</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid(int index)
    {
        return (index >= 0 && index < _width * _height);
    }

    /// <summary>
    /// 격자 좌표가 맵 안에 있는지 검사합니다.
    /// </summary>
    /// <param name="x">격자 X 좌표</param>
    /// <param name="y">격자 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InMap(int x, int y)
    {
        int localX = x - _startX;
        int localY = y - _startY;
        if (localX < 0) return false;
        if (localY < 0) return false;
        if (localX >= _width) return false;
        if (localY >= _height) return false;
        return true;
    }

    /// <summary>
    /// 격자 좌표가 맵 안에 있는지 검사합니다.
    /// </summary>
    /// <param name="pos">격자 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InMap(Vector2Int pos)
    {
        return InMap(pos.x, pos.y);
    }

    /// <summary>
    /// 월드 좌표가 맵 안에 있는지 검사합니다.
    /// </summary>
    /// <param name="x">월드 X 좌표</param>
    /// <param name="y">월드 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InMap(float x, float y)
    {
        int nX = Mathf.FloorToInt(x);
        int nY = Mathf.FloorToInt(y);
        return InMap(nX, nY);
    }

    /// <summary>
    /// 월드 좌표가 맵 안에 있는지 검사합니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InMap(Vector2 pos)
    {
        return InMap(pos.x, pos.y);
    }

    /// <summary>
    /// 월드 좌표를 맵 범위 안으로 클램프합니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ClampMap(Vector2 pos)
    {
        float startX = (float)_startX + K.SMALL_DISTANCE;
        float endX = (float)(_startX + _width) - K.SMALL_DISTANCE;
        float startY = (float)_startY + K.SMALL_DISTANCE;
        float endY = (float)(_startY + _height) - K.SMALL_DISTANCE;
        pos.x = Mathf.Clamp(pos.x, startX, endX);
        pos.y = Mathf.Clamp(pos.y, startX, endX);
        return pos;
    }

    /// <summary>
    /// 월드 좌표를 맵 범위 안으로 클램프합니다.
    /// </summary>
    /// <param name="x">월드 X 좌표</param>
    /// <param name="y">월드 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float x, float y) ClampMap(float x, float y)
    {
        float startX = (float)_startX + K.SMALL_DISTANCE;
        float endX = (float)(_startX + _width) - K.SMALL_DISTANCE;
        float startY = (float)_startY + K.SMALL_DISTANCE;
        float endY = (float)(_startY + _height) - K.SMALL_DISTANCE;
        x = Mathf.Clamp(x, startX, endX);
        y = Mathf.Clamp(y, startY, endY);
        return (x, y);
    }

    /// <summary>
    /// 격자 좌표를 맵 범위 안으로 클램프합니다.
    /// </summary>
    /// <param name="x">격자 X 좌표</param>
    /// <param name="y">격자 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2Int ClampMap(Vector2Int pos)
    {
        pos.x = Mathf.Clamp(pos.x, _startX, _startX + _width - 1);
        pos.y = Mathf.Clamp(pos.y, _startY, _startY + _height - 1);
        return pos;
    }

    /// <summary>
    /// 격자 좌표를 맵 범위 안으로 클램프합니다.
    /// </summary>
    /// <param name="x">격자 X 좌표</param>
    /// <param name="y">격자 Y 좌표</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int x, int y) ClampMap(int x, int y)
    {
        x = Mathf.Clamp(x, _startX, _startX + _width - 1);
        y = Mathf.Clamp(y, _startY, _startY + _height - 1);
        return (x, y);
    }
    #endregion

    #region ─────────────────────────▶ 공개 메서드 ◀─────────────────────────
    /// <summary>
    /// 생성자 → 외부에서 TileSingle[]을 주입해주는 구조
    /// </summary>
    public TileMap(TileSingle[] tiles, int width, int height, int startX, int startY)
    {
        _tiles = tiles;
        _width = width;
        _height = height;
        _startX = startX;
        _startY = startY;
    }

    /// <summary>
    /// 유닛이 점유한 격자를 순환할 수 있는 변수를 반환받습니다.
    /// </summary>
    /// <param name="pos">월드 좌표</param>
    /// <param name="size">유닛 크기</param>
    /// <returns></returns>
    public (int startX, int endX, int startY, int endY)
        GetOccupiedGrid(Vector2 pos, Vector2 size)
    {
        // 유닛 크기 결정
        float halfUnitX = size.x * K.GRID_SIZE_HALF;
        float halfUnitY = size.y * K.GRID_SIZE_HALF;
        // 점유한 격자 좌표의 시작과 끝
        int startX = Mathf.FloorToInt(pos.x - halfUnitX);
        int endX = Mathf.FloorToInt(pos.x + halfUnitX);
        int startY = Mathf.FloorToInt(pos.y - halfUnitY);
        int endY = Mathf.FloorToInt(pos.y + halfUnitY);
        (startX, startY) = ClampMap(startX, startY);
        (endX, endY) = ClampMap(endX, endY);
        // 완료
        return (startX, endX, startY, endY);
    }

    /// <summary>
    /// 유닛이 해당 좌표를 밟을 수 있는지 검사합니다.
    /// </summary>
    /// <param name="pos">유닛이 밟을 월드 좌표</param>
    /// <param name="size">유닛 크기</param>
    /// <returns></returns>
    public bool CheckCollision(Vector2 targetPos, Vector2 size)
    {
        var (startX, endX, startY, endY) = GetOccupiedGrid(targetPos, size);
        for (int y = startY; y <= endY; ++y)
        {
            for (int x = startX; x <= endX; ++x)
            {
                int index = GridToIndex(x, y);
                // 유효하지 않은 인덱스 or 이동 불가 타일
                if (!IsValid(index) || !IsMoveable(index))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 유닛이 이번 프레임이 밟게 될 최종 좌표를 반환합니다.
    /// </summary>
    /// <param name="curPos">현재 좌표</param>
    /// <param name="size">유닛 크기</param>
    /// <param name="dir">이동 방향</param>
    /// <param name="speed">초당 이동 속도</param>
    /// <returns></returns>
    public Vector2 GetValidPos(Vector2 curPos, Vector2 size, Vector2 dir, float speed)
    {
        // 방향 또는 이동 속도에 의해 위치 변화 없음
        if (dir == Vector2.zero || speed <= 0f)
        {
            return curPos;
        }
        // 목표 위치가 안전하다면 반환
        float movement = speed * Time.deltaTime;
        Vector2 desiredPos = curPos + (dir * speed); // 목표 위치
        if (!CheckCollision(desiredPos, size))
        {
            return desiredPos;
        }
        // AABB 충돌 로직
        Vector2 desiredX = new Vector2(desiredPos.x, curPos.y); // X축 이동 검사
        bool canMoveX = !CheckCollision(desiredX, size);
        Vector2 desiredY = new Vector2(curPos.x, desiredPos.y); // Y축 이동 검사
        bool canMoveY = !CheckCollision(desiredY, size);
        // 최종 좌표 계산
        Vector2 finalPos = new Vector2(
            canMoveX ? desiredPos.x : curPos.x,
            canMoveY ? desiredPos.y : curPos.y
        );
        return finalPos;
    }

    /// <summary>
    /// 유닛이 이번 프레임에 가져야 할 속도를 반환합니다.
    /// 물리 기반 이동이므로 FixedUpdate 전용입니다.
    /// </summary>
    /// <param name="curPos">현재 좌표</param>
    /// <param name="size">유닛 크기</param>
    /// <param name="dir">이동 방향</param>
    /// <param name="speed">초당 이동 속도</param>
    /// <returns></returns>
    public Vector2 GetValidVelocity(Vector2 curPos, Vector2 size, Vector2 dir, float speed)
    {
        // 방향 또는 이동 속도에 의해 위치 변화 없음
        if (dir == Vector2.zero || speed <= 0f)
        {
            return Vector2.zero;
        }
        // 예상 이동 위치
        float movement = speed * Time.fixedDeltaTime;
        Vector2 desiredPos = curPos + (dir * movement);
        // AABB 충돌 로직
        Vector2 desiredX = new Vector2(desiredPos.x, curPos.y); // X축 이동 검사
        bool canMoveX = !CheckCollision(desiredX, size);
        Vector2 desiredY = new Vector2(curPos.x, desiredPos.y); // Y축 이동 검사
        bool canMoveY = !CheckCollision(desiredY, size);
        // 최종 방향 계산
        Vector2 finalVelocity = new Vector2(
            canMoveX ? dir.x : 0,
            canMoveY ? dir.y : 0
        );
        return finalVelocity * speed;
    }
    #endregion
}
