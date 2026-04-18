using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

/// <summary>
/// 현재 씬에서 데이터를 수집하거나 로드합니다.
/// </summary>
public class PersistenceManager : GlobalSingleton<PersistenceManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private bool _isLoading = false;
    private bool _isSaving = false;
    private bool _isFirst = true;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool IsFirst { get => _isFirst; set => _isFirst = value; }

    /// <summary>
    /// 글로벌 데이터와 현재 씬 정보를 저장합니다.
    /// </summary>
    public void Save()
    {
        if (_isSaving)
        {
            UDebug.Print($"저장 작업 도중 저장이 중복 실행되었으므로 무시합니다.", LogType.Warning);
            return;
        }
        if (_isLoading)
        {
            UDebug.Print($"저장 작업 도중 로드가 실행되었으므로 무시합니다.", LogType.Warning);
            return;
        }
        _isSaving = true;
        EScene curScene = GameManager.Ins.Scene;
        UDebug.Print($"씬 {curScene}에서 저장을 시작합니다.");
        Stopwatch sw = new();
        sw.Start();
        SaveDataManager(curScene);
        SaveDynamicData(curScene);
        CollectObjectsAndSaveJson(curScene);
        sw.Stop();
        _isSaving = false;
        UDebug.Print($"씬 {curScene}의 저장이 완료되었습니다. ({(sw.ElapsedMilliseconds * 0.001):F2}s)");
    }

    /// <summary>
    /// 저장해둔 글로벌 데이터와 씬 정보를 로드합니다.
    /// </summary>
    public void Load()
    {
        if (_isSaving)
        {
            UDebug.Print($"불러오기 작업 도중 저장이 실행되었으므로 무시합니다.", LogType.Warning);
            return;
        }
        if (_isLoading)
        {
            UDebug.Print($"불러오기 작업 도중 로드가 중복 실행되었으므로 무시합니다.", LogType.Warning);
            return;
        }
        _isLoading = true;
        var dm = DataManager.Ins;
        var gm = GameManager.Ins;
        EScene prevScene = gm.Scene;
        UDebug.Print($"씬 {prevScene}에서 로드를 시작합니다.");
        Stopwatch sw = new();
        sw.Start();
        // 씬 로드 시작
        LoadDataManager(); // 글로벌 데이터
        EScene nextScene = dm.Player.CurPlayerScene();
        LoadDynamicData(nextScene);
        gm.LoadSceneAsyncWithFade((int)nextScene);
        // 씬 로드 종료
        sw.Stop();
        _isLoading = false;
        UDebug.Print($"씬 {nextScene}의 로드가 완료되었습니다. ({(sw.ElapsedMilliseconds * 0.001):F2}s)");
    }

    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        // 초기 데이터 먼저 불러오기
        LoadDataManager();
        // 이벤트 구독
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
        if (_isSaving)
        {
            UDebug.Print($"저장 작업 도중 저장이 중복 실행되었으므로 무시합니다.", LogType.Assert);
            return;
        }
        if (_isLoading)
        {
            UDebug.Print($"저장 작업 도중 로드가 실행되었으므로 무시합니다.", LogType.Error);
            return;
        }
        _isSaving = true;
        CollectObjectsAndSaveJson(ctx.prevScene);
        SaveDynamicData(ctx.prevScene);
        SaveDataManager(ctx.prevScene);
        _isSaving = false;
    }
    private void SceneChangeEndHandle(OnSceneLoadEnd ctx)
    {
        if (_isSaving)
        {
            UDebug.Print($"불러오기 작업 도중 저장이 실행되었으므로 무시합니다.", LogType.Assert);
            return;
        }
        ReadJsonAndLoadObjects(ctx.nextScene);
        LoadDynamicData(ctx.nextScene);
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
        // 디렉토리를 삭제하여 기존 세이브 파일 제거
        string dirPath = $"{Application.persistentDataPath}/{K.PRESISTENT_OBJECT_PATH}/{prevScene}";
        if (Directory.Exists(dirPath))
        {
            Directory.Delete(dirPath, true);
        }
        // 디렉토리 생성
        Directory.CreateDirectory(dirPath);
        // 모든 BaseMono 순회
        int length = gos.Length;
        string basePath = $"{Application.persistentDataPath}/{K.PRESISTENT_OBJECT_PATH}/{prevScene}";
        int success = 0;
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
            string path = $"{basePath}/{component.UnitId}@{component.UniqueId}.json";
            using (StreamWriter sw = new(path))
            {
                sw.Write(json);
            }
            success++;
        }
        UDebug.Print($"오브젝트 {success}개를 저장했습니다.");
    }

    // 알맞은 폴더 경로에서 Json을 읽어서 오브젝트를 생성합니다.
    private void ReadJsonAndLoadObjects(EScene nextScene)
    {
        // 폴더가 없다면 불러올 것이 없음
        string dirPath = $"{Application.persistentDataPath}/{K.PRESISTENT_OBJECT_PATH}/{nextScene}";
        if (!Directory.Exists(dirPath))
        {
            UDebug.Print($"{nextScene} 씬 폴더가 존재하지 않으므로 로드를 중단합니다.");
            return;
        }
        // 씬에 존재하는 모든 BaseMono 오브젝트 수집 및 딕셔너리 작성
        BaseMono[] gos = FindObjectsByType<BaseMono>(FindObjectsSortMode.None);
        Dictionary<string, ISaveable> saveableDict = new();
        int goCount = gos.Length;
        for (int i = 0; i < goCount; ++i)
        {
            BaseMono mono = gos[i];
            if (mono is not ISaveable saveable)
            {
                continue;
            }
            if (!saveableDict.TryAdd(saveable.UniqueId, saveable))
            {
                UDebug.Print($"모노 딕셔너리를 작성하는 도중 중복 등록이 발생했습니다." +
                    $"\n(UUID = {saveable.UniqueId})", LogType.Assert);
            }
        }
        // 폴더 경로에 존재하는 모든 Json 파일 가져오기
        string[] files = Directory.GetFiles(dirPath, "*.json");
        // 모든 파일을 순회하여 오브젝트 로드
        int fileCount = files.Length;
        if(fileCount <= 0)
        {
            UDebug.Print($"{nextScene} 씬에서 로드할 오브젝트 데이터가 존재하지 않습니다.");
            return;
        }
        var dm = DatabaseManager.Ins;
        for (int i = 0; i < fileCount; ++i)
        {
            string filePath = files[i];
            string fileName = Path.GetFileNameWithoutExtension(filePath); // 경로와 확장자 제거
            int index = fileName.IndexOf('@');
            // 잘못된 파일 형식
            if (index < 0)
            {
                continue;
            }
            string uniqueId = fileName.Substring(index + 1);
            string json = File.ReadAllText(filePath);
            // 씬에 존재하는 UUID
            if (saveableDict.TryGetValue(uniqueId, out ISaveable val))
            {
                val.LoadData(json);
                continue;
            }
            // 씬에 없으므로 생성하여 데이터 주입
            string unitId = fileName.Substring(0, index);
            GameObject prefab = dm.Unit(unitId)?.Prefab;
            if (prefab == null)
            {
                UDebug.Print($"UnitSO에 {unitId} ID를 가진 프리펩이 존재하지 않습니다.", LogType.Warning);
                continue;
            }
            // 생성 및 데이터 주입
            GameObject instance = UObject.Spawn(prefab, GameManager.ObjectRoot, true);
            if (instance.TryGetComponent(out ISaveable saveable))
            {
                saveable.UniqueId = uniqueId;
                saveable.UnitId = unitId;
                saveable.LoadData(json);
            }
            // 세이브 컴포넌트를 가져오지 못했으므로 다시 파괴
            else
            {
                UDebug.Print($"데이터를 로드하여 프리펩을 생성했으나 ISaveable을 발견하지 못했습니다.", LogType.Warning);
                UObject.Destroy(instance);
            }
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 (데이터 매니저) ◀─────────────────────────
    // DataManager의 데이터 일괄 저장
    private void SaveDataManager(EScene saveScene)
    {
        if(saveScene != EScene.Main && saveScene != EScene.Forest && saveScene != EScene.Cave)
        {
            UDebug.Print($"게임 씬이 아니므로 저장하지 않습니다. ({saveScene})");
            return;
        }
        DataManager m = DataManager.Ins;
        //
        var option = m.Option;
        SaveData(ref option);
        //
        m.Player.SaveSceneId(saveScene);
        var player = m.Player;
        SaveData(ref player);
        //
        var record = m.Record;
        record.SaveBeforeSerialize();
        SaveData(ref record);
        //
        SavedInventoryList invList = InventoryManager.Ins.GetSaveData();
        SaveData(ref invList);
    }

    // DataManager의 글로벌 데이터 일괄 로드
    private void LoadDataManager()
    {
        DataManager m = DataManager.Ins;
        var option = m.Option;
        LoadData(ref option);
        //
        var player = m.Player;
        LoadData(ref player);
        player.IsLoaded = true;
        //
        var farmland = m.Farmlands;
        LoadData(ref farmland);
        // 
        var record = m.Record;
        LoadData(ref record);
        record.LoadAfterDeserialize();
        //
        SavedInventoryList invList = new();
        LoadData(ref invList);
        InventoryManager.Ins.RestoreSaveDataEntry(invList);
    }

    // DataManager의 씬 데이터 일괄 저장
    private void SaveDynamicData(EScene prevScene)
    {
        DataManager m = DataManager.Ins;
        if(prevScene == EScene.Main)
        {
            var farmland = m.Farmlands;
            SaveData(ref farmland);
        }
    }

    // DataManager의 씬 데이터 일괄 로드
    private void LoadDynamicData(EScene nextScene)
    {
        DataManager m = DataManager.Ins;
        if(nextScene == EScene.Main)
        {
            var farmland = m.Farmlands;
            LoadData(ref farmland);
        }
    }
    /*
        구조체 적용법
        var player = mng.Player;
        LoadData(ref player);
        mng.Player = player;
    */

    // 단일 글로벌 데이터 저장하기
    private void SaveData<T>(ref T target)
    {
        // 저장하기 → AppData\LocalLow\<CompanyName>\<ProductName>
        string path = $"{Application.persistentDataPath}/{typeof(T).Name}.json";
        string json = JsonUtility.ToJson(target, true);
        using (StreamWriter sw = new(path))
        {
            sw.Write(json);
        }
    }

    // 단일 글로벌 데이터 불러오기
    private void LoadData<T>(ref T target)
    {
        // 불러오기 → AppData\LocalLow\<CompanyName>\<ProductName>
        string path = $"{Application.persistentDataPath}/{typeof(T).Name}.json";
        if (File.Exists(path))
        {
            using (StreamReader sr = new(path))
            {
                string json = sr.ReadToEnd();
                JsonUtility.FromJsonOverwrite(json, target);
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
