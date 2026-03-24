/// <summary>
/// 경작지의 상태를 나타내는 열거형 (테스트)
/// </summary>
public enum EFarmlandState
{

    None = 0,
    IdleLand = 1<<0,        //일반 흙
    SoiledLand = 1<<1,      //일궈진 흙
    SeededLand = 1 << 2,      //씨앗이 심어진 흙
    MoistLand = 1 << 3,       //축축한 흙
    GrownUp = 1 << 4         //성장 완료

}
