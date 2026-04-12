/// <summary>
/// 플레이어의 상태를 정의하는 열거형
/// </summary>
public enum EPlayerState : byte
{
    None = 0,
    Idle = 1,
    Walk = 2,
    Run = 3,
    Fishing = 4,
    Mining = 5,
    Logging = 6,
    Drinking = 7,
    Eating = 8,
    Sickle = 9,
    Shovel = 10,
}
