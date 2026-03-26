using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TestSubscrible : BaseMono
{
    [SerializeField] private TestObserver _parent;

    private void Handle(int amount)
    {
        UDebug.Print($"구독자가 호출되었다. {amount}");
    }

    private void OnEnable()
    {
        _parent.Amount.Bind(Handle);
    }

    private void OnDisable()
    {
        _parent.Amount.UnBind(Handle);
    }
}
