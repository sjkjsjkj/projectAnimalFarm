/// <summary>
/// 매 프레임 동작하는 월드 객체가 상속받는 추상화 클래스
/// </summary>
public abstract class InfoObject : Frameable
{
    public abstract void SetInfo(UnitSO data);
}
