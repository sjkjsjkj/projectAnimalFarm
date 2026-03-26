/// <summary>
/// 농작물의 런타임 데이터를 저장하는 구조체
/// </summary>
public struct CropWorldData
{
    public string id;
    public int stage; // 성장 단계
    public float elapsedTime; // 심어진 후 경과한 시간

    public CropWorldData(string id)
    {
        this.id = id;
        this.stage = 1;
        this.elapsedTime = 0f;
    }
}
