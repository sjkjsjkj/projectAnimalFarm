using UnityEngine;

/// <summary>
/// 키 코드 1~5로 플레이어 애니메이션 재생
/// </summary>
public class TestPlayerAnimation : Frameable
{
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnPlayerLogging.Publish(Vector2.zero, 1f);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnPlayerMining.Publish(Vector2.zero, 1f);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnPlayerSickle.Publish(Vector2.zero, 1f);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            OnPlayerShovel.Publish(Vector2.zero, 1f);
        }
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            OnPlayerFishing.Publish(Vector2.zero, 7f, UMath.IsProbability(50));
        }
    }
}
