using UnityEngine;

/// <summary>
/// 동물이 사육장의 먹이통의 위치를 구하기 위해 사육장에게 부착할 인터페이스.
/// </summary>
public interface IFoodProvider
{
    Transform GetFoodBoxPosition();
}
