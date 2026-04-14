/// <summary>
/// 런타임 데이터를 보관하는 매니저입니다.
/// </summary>
public class DataManager : GlobalSingleton<DataManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private OptionData _gameOption;
    private PlayerProvider _playerProvider;
    private FarmlandSaveData _farmlands;
    private RecordData _record;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public OptionData Option => _gameOption;

    /// <summary>
    /// 소리 설정에 접근합니다.
    /// </summary>
    public VolumeData Volume => _gameOption.Volume;

    /// <summary>
    /// 화면 해상도 설정에 접근합니다.
    /// </summary>
    public ScreenData Screen => _gameOption.Screen;

    /// <summary>
    /// 플레이어 런타임 데이터에 접근합니다.
    /// </summary>
    public PlayerProvider Player => _playerProvider;

    /// <summary>
    /// 경작지 배열을 가져옵니다. (세이브 로드 용도)
    /// </summary>
    public FarmlandSaveData Farmlands => _farmlands;

    /// <summary>
    /// 게임 기록에 접근합니다.
    /// </summary>
    public RecordData Record => _record;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized) return;
        // 옵션 초기화
        _gameOption = new();
        PlayerWorldSO playerSO = DatabaseManager.Ins.Player(Id.World_Player);
        _playerProvider = new(playerSO);
        _farmlands = new();
        _record = new();
        // 완료
        _isInitialized = true;
        UDebug.Print("데이터 매니저 초기화가 완료되었습니다.");
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    [System.Serializable]
    public class FarmlandSaveData
    {
        public Farmland[] farmlands;
    }
    #endregion
}
