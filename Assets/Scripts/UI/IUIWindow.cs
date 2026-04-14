/// <summary>
/// 공통 UI 창이 따라야 하는 규격입니다.
/// </summary>
public interface IUIWindow
{
    /// <summary>
    /// 현재 창이 열려 있는지 여부입니다.
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// 현재 창이 Esc로 닫힐 수 있는지 여부입니다.
    /// </summary>
    bool CanCloseWithEsc { get; }

    /// <summary>
    /// 창을 엽니다.
    /// </summary>
    void Open();

    /// <summary>
    /// 창을 닫습니다.
    /// </summary>
    void Close();
}
