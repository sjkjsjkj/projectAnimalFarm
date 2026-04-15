/// <summary>
/// 타일의 상태를 정의하는 열거형
/// </summary>
[System.Flags]
public enum ETileState : int
{
    None = 0,
    Moveable = 1 << 0, // 이동 가능
    CaveFloor = 1 << 1, // 동굴 바닥
    CaveRail = 1 << 2, // 동굴 레일
    ForestGrass = 1 << 3, // 숲 잔디
    WoodBridge = 1 << 4, // 나무 다리
    TownStoneRoad = 1 << 5, // 마을 돌길
    TownGrass = 1 << 6, // 마을 잔디
    TownFlowerGrass = 1 << 7, // 마을 꽃잔디
    TownDirtRoad = 1 << 8, // 마을 흙길
    TownFarmDirt = 1 << 9, // 마을 농장 흙
    TownSandRoad = 1 << 10, // 마을 모래길
}
