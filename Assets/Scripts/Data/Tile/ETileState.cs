/// <summary>
/// 타일의 상태를 정의하는 열거형
/// </summary>
[System.Flags]
public enum ETileState : int
{
    None = 0,
    Moveable = 1 << 0, // 이동 가능
    Farmable = 1 << 1, // 경작 가능
    Fishingable = 1 << 2, // 낚시 가능
    Buildable = 1 << 3, // 타일 위에 건설 가능
    Interactable = 1 << 4, // 상호작용 가능
    SeaFishingable = 1 << 5, // 바다 낚시 가능
}
