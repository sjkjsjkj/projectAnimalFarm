/// <summary>
/// 현재 경작지 주변의 경작지들과 비교하여 자연스러운 스프라이트를 생성하기 위한 이벤트의 구조체 입니다.
/// </summary>
public struct FarmlandConnetionChangeStruct
{
    public readonly uint connectionDir;
    public readonly EFarmlandState state;
    public readonly int pos;

    public FarmlandConnetionChangeStruct(uint connectionDir, EFarmlandState state, int pos)
    {
        this.connectionDir = connectionDir;
        this.state = state;
        this.pos = pos;
    }
}
