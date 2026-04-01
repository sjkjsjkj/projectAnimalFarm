using UnityEngine;

/// <summary>
/// (현재 씬 기준) - 키로 로드, = 키로 세이브
/// </summary>
public class TestSaveLoad : Frameable
{
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        // 디버그 키 안내
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            UDebug.Print($"데이터 저장 : = (등호)" +
                $"\n데이터 로드 : - (마이너스)");
        }

        // 씬 전환
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            PersistenceManager.Ins.Save();
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            PersistenceManager.Ins.Load();
        }
    }
}
