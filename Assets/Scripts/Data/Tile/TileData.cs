/// <summary>
/// 구조체의 설계 의도입니다.
/// </summary>
public class TileData
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private int _id;
    private string _name;
    private int _size;
    private uint _state;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int Id => _id;
    public string Name => _name;
    public int Size => _size;
    public uint State => _state;

    #endregion

    #region ─────────────────────────▶   생성자  ◀─────────────────────────
    public TileData(int id, string name, int size, uint state)
    {
        _id = id; _name = name; _size = size; _state = state;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion
}
