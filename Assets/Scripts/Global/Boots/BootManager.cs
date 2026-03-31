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
        // 순서 의존성 없음 ↓
        var inputManager = UObject.AddComponent<InputManager>(root); // Input Action을 읽음
        inputManager.Initialize();
        var tileIdToState = UObject.AddComponent<TileIdToState>(root); // 구글 시트를 읽음
        tileIdToState.Initialize();
        var gameManager = UObject.AddComponent<GameManager>(root);
        gameManager.Initialize();
        // TileIdToState 이후 실행 ↓
        var tileManager = UObject.AddComponent<TileManager>(root);
        tileManager.Initialize();
        var databaseManager = UObject.AddComponent<DatabaseManager>(root); // 리소스 폴더의 테이블 SO를 읽음
        databaseManager.Initialize();
        // DatabaseManager 이후 실행 ↓
        var dataManager = UObject.AddComponent<DataManager>(root);
        dataManager.Initialize();
        // DataManager 이후 실행 ↓
        var persistenceManager = UObject.AddComponent<PersistenceManager>(root);
        persistenceManager.Initialize();
        var bgmManager = UObject.AddComponent<BgmManager>(root); // 옵션(Data), SO(Database)를 읽음
        bgmManager.Initialize();
        // 다른 매니저들을 위해 마지막에 실행 ↓
        var frameManager = UObject.AddComponent<FrameManager>(root);
        frameManager.Initialize();
        yield return null;
        UDebug.Print("BootSequence : 초기화 완료");
        _co = null;
    }
}
