using System.Runtime.CompilerServices;

/// <summary>
/// 2D 격자 맵의 타일 상태를 관리하는 클래스입니다.
/// </summary>
public class TileMap
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly TileSingle[] _tiles;
    private readonly int _width;
    private readonly int _height;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
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

    /// <summary>
    /// 상태가 존재하지 않는 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNone(int index)
    {
        if (UGrid.InMap(index, _width, _height))
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
        if (UGrid.InMap(index, _width, _height))
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
        if (UGrid.InMap(index, _width, _height))
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
        if (UGrid.InMap(index, _width, _height))
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
        if (UGrid.InMap(index, _width, _height))
        {
            return (_tiles[index].state & ETileState.Buildable) != 0;
        }
        return false;
    }

    /// <summary>
    /// 파괴가 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBreakable(int index)
    {
        if (UGrid.InMap(index, _width, _height))
        {
            return (_tiles[index].state & ETileState.Breakable) != 0;
        }
        return false;
    }

    /// <summary>
    /// 상호작용이 가능한 타일
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInteractable(int index)
    {
        if (UGrid.InMap(index, _width, _height))
        {
            return (_tiles[index].state & ETileState.Interactable) != 0;
        }
        return false;
    }
    #endregion

    #region ─────────────────────────▶ 공개 메서드 ◀─────────────────────────
    /// <summary>
    /// 생성자 → 외부에서 TileSingle[]을 주입해주는 구조
    /// </summary>
    public TileMap(int width, int height, TileSingle[] tiles)
    {
        _width = width;
        _height = height;
        _tiles = tiles;
    }
    #endregion
}
