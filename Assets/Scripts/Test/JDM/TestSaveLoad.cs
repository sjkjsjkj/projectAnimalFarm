using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TestSaveLoad : BaseMono
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            PersistenceManager.Ins.Save();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            PersistenceManager.Ins.Load();
        }
    }
}
