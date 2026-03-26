using UnityEngine;

/// <summary>
/// 효과를 구현하는 SO가 상속받아야 할 클래스
/// </summary>
public abstract class EffectSO : ScriptableObject
{
    public abstract void Execute(Transform user, Transform target);
}
