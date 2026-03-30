using System.Collections;
using UnityEngine;

/// <summary>
/// 초기화 순서를 제어합니다.
/// </summary>
public class BootManager : MonoBehaviour
{
    private Coroutine _co;
    public void StartBootSequence(GameObject root)
    {
        if(_co != null)
        {
            UDebug.Print("StartBootSequence()가 중복 호출되었습니다.", LogType.Assert);
            return;
        }
        _co = StartCoroutine(CoInitialize(root));
    }

    private IEnumerator CoInitialize(GameObject root)
    {
        UDebug.Print("BootSequence : 초기화 중 ....");
        // 매니저 생성 및 초기화
        var frameManager = root.AddComponent<FrameManager>();
        frameManager.Initialize();
        var inputManager = root.AddComponent<InputManager>();
        inputManager.Initialize();
        var tileIdToState = root.AddComponent<TileIdToState>();
        tileIdToState.Initialize();
        var tileManager = root.AddComponent<TileManager>();
        tileManager.Initialize();
        var gameManager = root.AddComponent<GameManager>();
        gameManager.Initialize();
        var databaseManager = root.AddComponent<DatabaseManager>();
        databaseManager.Initialize();
        var dataManager = root.AddComponent<DataManager>();
        dataManager.Initialize();
        // 
        yield return null;
        UDebug.Print("BootSequence : 초기화 완료");
        _co = null;
    }
}
