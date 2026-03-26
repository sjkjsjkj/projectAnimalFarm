using UnityEngine;

/// <summary>
/// 플레이어가 생성되었을 때 (초기, 사망, 씬 이동)
/// </summary>
public readonly struct OnPlayerSpawn
{
    public readonly Transform tr;

    public OnPlayerSpawn(Transform tr)
    {
        this.tr = tr;
    }

    /// <param name="tr">플레이어의 트랜스폼</param>
    public static void Publish(Transform tr)
    {
        EventBus<OnPlayerSpawn>.Publish(new(tr));
    }
}
