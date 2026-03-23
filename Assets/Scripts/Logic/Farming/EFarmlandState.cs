/// <summary>
/// 
/// </summary>
public enum EFarmlandState
{
    None=999,
    IdleLand =0,        //일반 흙
    SoiledLand =1,      //일궈진 흙
    SeededLand =2,      //씨앗이 심어진 흙
    MoistLand= 3,       //축축한 흙
    GrownUp = 4         //성장 완료
}
