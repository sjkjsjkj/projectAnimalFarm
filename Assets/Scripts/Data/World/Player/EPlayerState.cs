/// <summary>
/// 플레이어의 상태를 정의하는 열거형
/// </summary>
public enum EPlayerState : byte
{
    None = 0,
    Idle = 1,
    Move = 2,
    Interact = 3,
    ToolAction = 4,
    Dead = 5
}
