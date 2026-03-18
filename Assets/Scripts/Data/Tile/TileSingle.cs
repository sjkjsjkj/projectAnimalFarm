/// <summary>
/// 타일 하나의 정보를 담는 구조체입니다.
/// </summary>
public struct TileSingle
{
    public int id;
    public int size;
    public ETileState state;

    public TileSingle(string id, int size, ETileState state)
    {
        this.id = id.GetHashCode();
        this.size = size;
        this.state = state;
    }
}
