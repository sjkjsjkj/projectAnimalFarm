/// <summary>
/// 준수하면 Esc에 의해 종료를 관리받을 수 있습니다.
/// 들어오거나 나갈 때 Enter, Exit도 호출해주셔야 합니다.
/// </summary>
public interface IEscClosable
{
    void CloseUi();
}
