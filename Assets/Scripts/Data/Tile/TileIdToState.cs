using UnityEngine;

/// <summary>
/// 타일 ID로 타일 상태에 접근할 수 있습니다.
/// </summary>
public class TileIdToState : GlobalSingleton<TileIdToState>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private ETileState[] _idToState;
    private int _lastId;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    /// <summary>
    /// 클래스에 인덱서 문법으로 접근
    /// </summary>
    /// <param name="index">타일 ID</param>
    /// <returns></returns>
    public ETileState this[int index]
    {
        get
        {
            if(index < 0 || index > _lastId)
            {
                UDebug.Print($"[TileIdToState] 유효하지 않은 인덱스 접근 ({index})", LogType.Error);
                return ETileState.None;
            }
            return _idToState[index];
        }
    }

    // 클래스 생성 시 초기화
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        BuildTileIdToState();
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 초기화 진입점
    private void BuildTileIdToState()
    {
        UDebug.Print($"[TileIdToState] 배열 생성을 시작합니다.");
        (TextAsset sheet, string sheetData) = LoadCSV();
        if (sheetData != null)
        {
            SheetToArray(sheetData);
            UDebug.Print($"[TileIdToState] 배열 생성이 완료되었습니다.");
            Resources.UnloadAsset(sheet);
        }
    }

    // 리소스 폴더에서 로드한 CSV를 읽어서 검증 후 문자열 생성
    private (TextAsset sheet, string text) LoadCSV()
    {
        TextAsset csvData = Resources.Load<TextAsset>(K.TILE_RESOURCE_SHEET_PATH);
        // 시트를 탐색할 수 없음
        if(csvData == null)
        {
            UDebug.Print($"[TileIdToState] Resources/{K.TILE_RESOURCE_SHEET_PATH} 경로에서 타일 시트를 찾을 수 없습니다.", LogType.Assert);
            return (null, null);
        }
        // 시트가 비어있음
        string sheetData = csvData.text;
        if (string.IsNullOrEmpty(sheetData) || string.IsNullOrWhiteSpace(sheetData))
        {
            UDebug.Print($"[TileIdToState] 타일 시트 데이터가 존재하지 않습니다.", LogType.Assert);
            return (null, null);
        }
        // 타일 시트 맞나 확인
        if (!sheetData.Contains("Tileset"))
        {
            UDebug.Print($"[TileIdToState] 유효한 타일 시트 데이터가 아닙니다.", LogType.Assert);
            return (null, null);
        }
        // 검증 완료
        return (csvData, sheetData);
    }

    // 방대한 문자열을 받아서 IdToState 배열을 빌드
    private void SheetToArray(string sheetData)
    {
        //행과 열
        string[] rows = sheetData.Replace("\r", "").Split('\n');
        int rowsLength = rows.Length;
        // 배열 초기화
        _idToState = new ETileState[rowsLength];
        UDebug.Print($"[TileIdToState] 배열 초기화 (크기 = {rowsLength})");
        // 모든 행 순회
        int lastId = -1;
        for (int i = 1; i < rowsLength; ++i)
        {
            string[] cols = rows[i].Split(',');
            int colsLength = cols.Length;
            // 유효하지 않은 ID가 있다면 거르기
            if (!int.TryParse(cols[K.TILE_CSV_ID], out int id))
            {
                UDebug.Print($"[TileIdToState] 유효하지 않은 ID로 판단했습니다. ({cols[K.TILE_CSV_ID]})");
                continue;
            }
            // ID 잘못해서 크게 입력했는지 확인해주기
            if (id > rowsLength)
            {
                UDebug.Print($"[TileIdToState] ID가 행 길이보다 큽니다.", LogType.Assert);
                _idToState = null;
                return;
            }
            // 타일 상태 빌드
            ETileState state = BuildTileState(cols);
            _idToState[id] = state;
            if(id > lastId) // 혹시나 추가한 조건식
            {
                lastId = id;
            }
        }
        // 유효한 동작이 하나도 없는 심각한 상황 발생
        if (lastId < 0)
        {
            UDebug.Print($"[TileIdToState] ID를 전혀 읽지 못했습니다. null을 대입합니다.");
            _idToState = null;
            return;
        }
        _lastId = lastId;
        // 후처리 (배열 크기 맞추기)
        int desiredSize = lastId + 1;
        System.Array.Resize(ref _idToState, desiredSize);
        UDebug.Print($"[TileIdToState] 배열 재할당 및 카피 ({rowsLength} → {desiredSize})");
    }

    // 열 문자열을 받아서 타일 상태를 가려내 반환
    private ETileState BuildTileState(string[] cols)
    {
        ETileState state = ETileState.None;
        for (int i = K.TILE_CSV_STATE_START; i <= K.TILE_CSV_STATE_END; ++i)
        {
            if (cols[i].Trim() == "1")
            {
                state |= (ETileState)(1 << (i - K.TILE_CSV_STATE_START));
            }
        }
        return state;
    }
    #endregion
}
