using System.IO;
using UnityEngine;

/// <summary>
/// 현재 씬에서 데이터를 수집하거나 로드합니다.
/// </summary>
public class PersistenceManager : GlobalSingleton<PersistenceManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        // 초기 데이터 먼저 불러오기
        LoadDataManager();
        // 
        EventBus<OnSceneLoadStart>.Subscribe(SceneChangeStartHandle);
        EventBus<OnSceneLoadEnd>.Subscribe(SceneChangeEndHandle);
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 씬 전환 핸들
    private void SceneChangeStartHandle(OnSceneLoadStart ctx)
    {
        ReadJsonAndLoadObjects(ctx.nextScene);
    }
    private void SceneChangeEndHandle(OnSceneLoadEnd ctx)
    {
        CollectObjectsAndSaveJson(ctx.prevScene);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 (오브젝트) ◀─────────────────────────
    // 저장 대상인 오브젝트를 탐색하여 저장 함수를 호출시킵니다.
    private void CollectObjectsAndSaveJson(EScene prevScene)
    {
        // 씬에 존재하는 모든 BaseMono 오브젝트 수집
        BaseMono[] gos = FindObjectsByType<BaseMono>(FindObjectsSortMode.None);
        // 하나도 수집되지 않았을 경우 방어 코드
        if (gos == null || gos.Length <= 0)
        {
            return;
        }
        // 디렉토리 존재를 보장하기
        string dirPath = $"{Application.persistentDataPath}/{K.PRESISTENT_OBJECT_PATH}/{prevScene}";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        // 모든 BaseMono 순회
        int length = gos.Length;
        string basePath = $"{Application.persistentDataPath}/{K.PRESISTENT_OBJECT_PATH}/{prevScene}";
        for (int i = 0; i < length; ++i)
        {
            // 저장 컴포넌트 가져오기 시도
            if (!gos[i].TryGetComponent(out ISaveable component))
            {
                continue;
            }
            // Json 가져오기 시도
            string json = component.SaveData();
            if (json.IsEmpty())
            {
                UDebug.Print($"컴포넌트({component.UniqueId})에게서 저장 데이터를 가져오려 했지만 빈 문자열을 받았습니다.", LogType.Warning);
                continue;
            }
            // 고유 ID로 저장
            string path = $"{basePath}/{component.UniqueId}.json";
            using (StreamWriter sw = new(path))
            {
                sw.Write(json);
            }
        }
    }

    // 씬을 통해 알맞은 폴더 경로에서 Json을 읽어서 오브젝트를 생성합니다.
    // 
    private void ReadJsonAndLoadObjects(EScene nextScene)
    {
        // 폴더 경로에 존재하는 모든 Json 읽기
        // 씬에 존재하는 모든 BaseMono 오브젝트 수집
        BaseMono[] gos = FindObjectsByType<BaseMono>(FindObjectsSortMode.None);
        // 하나도 수집되지 않았을 경우 방어 코드
        string dirPath = $"{Application.persistentDataPath}/{K.PRESISTENT_OBJECT_PATH}/{nextScene}";
        if (gos == null || gos.Length <= 0 || !Directory.Exists(dirPath))
        {
            return;
        }
        // 모든 BaseMono 순회
        int length = gos.Length;
        string basePath = $"{Application.persistentDataPath}/{K.PRESISTENT_OBJECT_PATH}/{nextScene}";
        for (int i = 0; i < length; ++i)
        {
            // 저장 컴포넌트 가져오기 시도
            if (!gos[i].TryGetComponent(out ISaveable component))
            {
                continue;
            }
            // Json 가져오기 시도
            string json = component.SaveData();
            if (json.IsEmpty())
            {
                UDebug.Print($"컴포넌트({component.UniqueId})에게서 저장 데이터를 가져오려 했지만 빈 문자열을 받았습니다.", LogType.Warning);
                continue;
            }
            // 고유 ID로 저장
            string path = $"{basePath}/{component.UniqueId}.json";
            using (StreamWriter sw = new(path))
            {
                sw.Write(json);
            }
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 (데이터 매니저) ◀─────────────────────────
    // DataManager의 데이터 일괄 저장
    private void SaveDataManager()
    {
        DataManager manager = DataManager.Ins;
        var option = manager.Option;
        SaveData(ref option);
        var player = manager.Player;
        SaveData(ref player);
    }

    // DataManager의 데이터 일괄 로드
    private void LoadDataManager()
    {
        DataManager manager = DataManager.Ins;
        var option = manager.Option;
        LoadData(ref option);
        var player = manager.Player;
        LoadData(ref player);
    }
    /*
        구조체 적용법
        var player = mng.Player;
        LoadData(ref player);
        mng.Player = player;
    */

    // 직렬화 가능한 클래스나 구조체를 경로에 저장합니다.
    private void SaveData<T>(ref T target)
    {
        // 값 형식 & 참조 형식 모두 아닐 경우
        if (!(typeof(T).IsValueType || typeof(T).IsClass))
        {
            UDebug.Print($"{target}은 저장 대상(값 형식 or 클래스)이 아닙니다.", LogType.Assert);
            return;
        }
        // 로드 → AppData\LocalLow\<CompanyName>\<ProductName>
        string path = $"{Application.persistentDataPath}/{typeof(T).Name}.json";
        string json = JsonUtility.ToJson(target, true);
        using (StreamWriter sw = new(path))
        {
            sw.Write(json);
        }
    }

    // 클래스나 구조체의 이름으로 
    private void LoadData<T>(ref T target)
    {
        // 값 형식 & 참조 형식 모두 아닐 경우
        if (!(typeof(T).IsValueType || typeof(T).IsClass))
        {
            UDebug.Print($"{target}은 불러오기 대상(값 형식 or 클래스)이 아닙니다.", LogType.Assert);
            return;
        }
        // 불러오기 → AppData\LocalLow\<CompanyName>\<ProductName>
        string path = $"{Application.persistentDataPath}/{typeof(T).Name}.json";
        if (File.Exists(path))
        {
            using (StreamReader sr = new(path))
            {
                string json = sr.ReadToEnd();
                T data = JsonUtility.FromJson<T>(json);
                target = data;
            }
        }
        // 파일이 존재하지 않음
        else
        {
            UDebug.Print($"경로({path})에 파일이 존재하지 않습니다.", LogType.Warning);
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnDisable()
    {
        EventBus<OnSceneLoadStart>.Unsubscribe(SceneChangeStartHandle);
        EventBus<OnSceneLoadEnd>.Unsubscribe(SceneChangeEndHandle);
    }
    #endregion
}
