using UnityEngine;

/// <summary>
/// 정적 데이터를 보관하는 매니저입니다.
/// </summary>
public class DatabaseManager : GlobalSingleton<DatabaseManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    // 테이블
    private BaitItemTableSO[] _baitItemTables;
    private SeedItemTableSO[] _seedItemTables;
    private FeedItemTableSO[] _feedItemTables;
    private SpecialItemTableSO[] _specialItemTables;
    private StaticItemTableSO[] _staticItemTables;
    private ToolItemTableSO[] _toolItemTables;
    private AnimalWorldTableSO[] _animalWorldTables;
    private CropWorldTableSO[] _cropWorldTables;
    private FieldWorldTableSO[] _fieldWorldTables;
    private NpcWorldTableSO[] _npcWorldTables;
    private StaticWorldTableSO[] _staticWorldTables;
    private SoundTableSO[] _soundTables;
    private PlayerWorldTableSO[] _playerWorldTables;
    // 프리펩
    private SoundEmitter _soundEmiiterPrefab;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    /// <summary>
    /// 미끼 아이템의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">미끼 아이템 ID</param>
    public BaitItemSO BaitItem(string id)
        => FindData<BaitItemTableSO, BaitItemSO>(_baitItemTables, id);

    /// <summary>
    /// 씨앗 아이템의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">씨앗 아이템 ID</param>
    public SeedItemSO SeedItem(string id)
        => FindData<SeedItemTableSO, SeedItemSO>(_seedItemTables, id);

    /// <summary>
    /// 먹이 아이템의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">먹이 아이템 ID</param>
    public FeedItemSO FeedItem(string id)
        => FindData<FeedItemTableSO, FeedItemSO>(_feedItemTables, id);

    /// <summary>
    /// 특수 아이템의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">특수 아이템 ID</param>
    public SpecialItemSO SpecialItem(string id)
        => FindData<SpecialItemTableSO, SpecialItemSO>(_specialItemTables, id);

    /// <summary>
    /// 일반적인 아이템의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">일반 아이템 ID</param>
    public StaticItemSO StaticItem(string id)
        => FindData<StaticItemTableSO, StaticItemSO>(_staticItemTables, id);

    /// <summary>
    /// 도구 아이템의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">도구 아이템 ID</param>
    public ToolItemSO ToolItem(string id)
        => FindData<ToolItemTableSO, ToolItemSO>(_toolItemTables, id);

    /// <summary>
    /// 동물 월드 객체의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">동물 월드 ID</param>
    public AnimalWorldSO AnimalWorld(string id)
        => FindData<AnimalWorldTableSO, AnimalWorldSO>(_animalWorldTables, id);

    /// <summary>
    /// 작물 월드 객체의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">작물 월드 ID</param>
    public CropWorldSO CropWorld(string id)
        => FindData<CropWorldTableSO, CropWorldSO>(_cropWorldTables, id);

    /// <summary>
    /// 필드 객체의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">필드 객체 ID</param>
    public FieldWorldSO FieldWorld(string id)
        => FindData<FieldWorldTableSO, FieldWorldSO>(_fieldWorldTables, id);

    /// <summary>
    /// NPC의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">NPC ID</param>
    public NpcWorldSO NpcWorld(string id)
        => FindData<NpcWorldTableSO, NpcWorldSO>(_npcWorldTables, id);

    /// <summary>
    /// 일반적인 월드 객체의 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">월드 객체 ID</param>
    public StaticWorldSO StaticWorld(string id)
        => FindData<StaticWorldTableSO, StaticWorldSO>(_staticWorldTables, id);

    /// <summary>
    /// 오디오의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">사운드 ID</param>
    public SoundSO Sound(string id)
        => FindData<SoundTableSO, SoundSO>(_soundTables, id);

    /// <summary>
    /// 플레이어의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">플레이어 ID</param>
    public PlayerWorldSO Player(string id)
        => FindData<PlayerWorldTableSO, PlayerWorldSO>(_playerWorldTables, id);

    /// <summary>
    /// 사운드 이미터 프리펩을 반환합니다.
    /// </summary>
    public SoundEmitter SoundPrefab() => GetSafePrefab(_soundEmiiterPrefab);
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized) return;
        // 모든 테이블 로드
        _baitItemTables = LoadTables<BaitItemTableSO, BaitItemSO>();
        _seedItemTables = LoadTables<SeedItemTableSO, SeedItemSO>();
        _feedItemTables = LoadTables<FeedItemTableSO, FeedItemSO>();
        _specialItemTables = LoadTables<SpecialItemTableSO, SpecialItemSO>();
        _staticItemTables = LoadTables<StaticItemTableSO, StaticItemSO>();
        _toolItemTables = LoadTables<ToolItemTableSO, ToolItemSO>();
        _animalWorldTables = LoadTables<AnimalWorldTableSO, AnimalWorldSO>();
        _cropWorldTables = LoadTables<CropWorldTableSO, CropWorldSO>();
        _fieldWorldTables = LoadTables<FieldWorldTableSO, FieldWorldSO>();
        _npcWorldTables = LoadTables<NpcWorldTableSO, NpcWorldSO>();
        _staticWorldTables = LoadTables<StaticWorldTableSO, StaticWorldSO>();
        _soundTables = LoadTables<SoundTableSO, SoundSO>();
        _playerWorldTables = LoadTables<PlayerWorldTableSO, PlayerWorldSO>();
        // 프리펩 로드
        _soundEmiiterPrefab = LoadPrefab<SoundEmitter>(K.NAME_SOUND_EMITTER);
        // 완료
        _isInitialized = true;
        UDebug.Print("모든 테이블 로드가 완료되었습니다.");
    }

    // 프리펩을 컴포넌트로 반환합니다.
    private T LoadPrefab<T>(string prefabName) where T : Component
    {
        if (prefabName.IsEmpty())
        {
            UDebug.Print($"리소스 폴더에서 로드할 프리펩 이름이 비어있습니다.", LogType.Assert);
            return null;
        }
        T prefab = Resources.Load<T>($"{K.PREFAB_RESOURCE_PATH}/{prefabName}");
        if (prefab == null)
        {
            UDebug.Print($"{prefabName} 이름을 가진 프리펩을 Resources/{K.PREFAB_RESOURCE_PATH}에서 찾을 수 없습니다." +
                $"\n또는 {typeof(T).Name} 컴포넌트가 부착되지 않았습니다.", LogType.Assert);
            return null;
        }
        return prefab;
    }

    // 리소스 폴더에서 특정 타입의 모든 테이블 SO를 찾아 배열로 반환
    private TTable[] LoadTables<TTable, TData>()
        where TTable : TableSO<TData> where TData : BaseSO
    {
        TTable[] tables = Resources.LoadAll<TTable>(K.TABLE_RESOURCE_PATH);
        // 탐색 실패
        if (tables == null || tables.Length <= 0)
        {
            UDebug.Print($"경로({K.TABLE_RESOURCE_PATH})에서 테이블({typeof(TTable).Name})을 찾을 수 없습니다.", LogType.Warning);
        }
        // 탐색 성공
        else
        {
            for (int i = 0; i < tables.Length; ++i)
            {
                tables[i].Initialize();
            }
            UDebug.Print($"경로({K.TABLE_RESOURCE_PATH})에서 테이블({typeof(TTable).Name})을 찾았습니다.");
        }
        return tables;
    }

    // Null 검사해서 프리펩 가져오기
    private T GetSafePrefab<T>(T prefab) where T : Component
    {
        if(prefab == null)
        {
            UDebug.Print($"가져올 {typeof(T).Name} 타입 프리펩이 존재하지 않습니다.");
            return null;
        }
        return prefab;
    }

    // 테이블에서 ID로 SO 인스턴스를 가져옵니다.
    private TData FindData<TTable, TData>(TTable[] tables, string id)
        where TTable : TableSO<TData>
        where TData : BaseSO
    {
        // 혹시나 방어 코드
        if (tables == null || tables.Length == 0)
        {
            UDebug.Print($"테이블을 찾을 수 없습니다. ({typeof(TTable).Name})", LogType.Assert);
            return null;
        }
        // 해당 테이블을 모두 순회하며 데이터 탐색
        for (int i = 0; i < tables.Length; i++)
        {
            TData data = tables[i].GetSO(id);
            // 탐색을 성공하여 SO 반환
            if (data != null)
            {
                return data;
            }
        }
        // 테이블에 ID가 존재하지 않음
        UDebug.Print($"테이블({typeof(TTable).Name})에서 SO 에셋을 찾을 수 없습니다.(ID = {id})", LogType.Assert);
        return null;
    }
    #endregion
}
