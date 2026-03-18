/// <summary>
/// 작물의 성장 상태를 정의하는 열거형
/// </summary>
public enum ECropState : byte
{
    None = 0,
    Seed = 1, // 씨앗 상태
    Growing = 2, // 성장 중
    Harvestable = 3, // 수확 가능
    Wither = 4, // 시듦
}
