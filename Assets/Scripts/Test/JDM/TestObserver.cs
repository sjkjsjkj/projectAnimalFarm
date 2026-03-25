using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TestObserver : BaseMono
{
    private int _number = 0;

    public Observable<int> Amount { get; } = new();

    [ContextMenu("값 수정해보기")]
    private void ValueChange()
    {
        _number++;
        Amount.Value = _number;
    }
}
