/// <summary>
/// 타일 하나의 정보를 담는 구조체입니다.
/// </summary>
public struct TileSingle
{
    public int id;
    public ETileState state;

    public TileSingle(int id, int size, ETileState state)
    {
        this.id = id;
        this.state = state;
    }
}
