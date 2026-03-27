using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 씬에서 작업한 타일맵을 Json 파일로 추출
/// </summary>
public class TilemapExporter : EditorWindow
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Tilemap _targetTileMap; // 읽어올 타일맵
    private TextAsset _csvData; // 이름이 담긴 CSV 파일 (ID와 연동용)
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 유니티 상단 메뉴바에 버튼 만들기
    [MenuItem("Tools/Build TileIdMap.json")]
    public static void ShowWindow()
    {
        // 해당 클래스 타입을 찾아서 탭 생성
        GetWindow<TilemapExporter>("TileData.csv + Tilemap → TileIdMap.json");
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 구글 시트를 읽어와서 이름 → ID 변환을 수행해주는 딕셔너리를 빌드하고 반환한다.
    private Dictionary<string, int> BuildFileToDict(TextAsset csv, Tilemap tileMap)
    {
        // 방어 코드
        if (UDebug.IsNull(csv) || UDebug.IsNull(tileMap))
        {
            return null;
        }
        // 셀은 콤마로 구분, 줄은 \n 문자로 구분
        // 파일 이름 → ID 변환해주는 딕셔너리 임시 생성
        Dictionary<string, int> fileToId = new();
        string[] csvLines = _csvData.text.Split('\n');
        int length = csvLines.Length;
        int success = 0;
        // 헤더 제외 루프
        for (int i = 1; i < length; ++i)
        {
            string line = csvLines[i];
            // 비어있는거 거르기
            if (string.IsNullOrEmpty(line)) continue;
            if (string.IsNullOrWhiteSpace(line)) continue;

            // 콤마 기준으로 쪼개서 이름만 가져오기 (B열 → 1번 인덱스)
            string[] cols = line.Split(',');

            // 안전한 ID 가져오기
            int colLength = cols.Length;
            if (colLength <= K.TILE_CSV_ID) continue;
            if (!int.TryParse(cols[K.TILE_CSV_ID], out int id)) continue;

            // 안전한 이름 가져오기
            string name = cols[K.TILE_CSV_NAME].Trim();
            if (colLength <= K.TILE_CSV_NAME) continue;
            if (string.IsNullOrEmpty(name)) continue;
            if (string.IsNullOrWhiteSpace(name)) continue;

            // 문제가 없으므로 딕셔너리 등록
            if (fileToId.TryAdd(name, id))
            {
                success++;
            }
            // 알 수 없는 이유로 중복 등록 발생
            else
            {
                UDebug.Print($"File To ID 중복 등록이 발생했습니다. (Name = {name}, ID = {id})", LogType.Assert);
            }
        }
        UDebug.Print($"딕셔너리를 생성했습니다. (성공한 개수 = {success})");
        return fileToId;
    }

    // 타일맵을 읽어서 저장용 클래스를 빌드하여 반환
    private TileIdMapSaveData BuildSaveData(Dictionary<string, int> fileToDict)
    {
        // 최소 좌표 + 크기로 영역을 정의
        // 빈 공간을 잘라내고 가능한 작아진다.
        BoundsInt bounds = _targetTileMap.cellBounds;
        int width = bounds.size.x;
        int height = bounds.size.y;

        // 평탄화된 배열 생성 및 순회
        int[] map = new int[width * height];
        int success = 0;
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                // 로컬 좌표의 타일 정보 가져오기
                Vector3Int pos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, K.GRID_Z_DEPTH);
                TileBase tile = _targetTileMap.GetTile(pos);
                int index = (y * width) + x;
                // 아무것도 없는 타일
                if (tile == null)
                {
                    map[index] = 5150; // 봄 잔디 0번 (밟을 수 있는 땅)
                }
                // 무언가 있는 타일
                else
                {
                    // 연동 성공
                    if (fileToDict.TryGetValue(tile.name, out int id))
                    {
                        map[index] = id;
                        success++;
                    }
                    // 딕셔너리에 없음
                    else
                    {
                        UDebug.Print($"타일 맵 인덱스({index})와 이름이 일치하는 시트 데이터가 없습니다.", LogType.Assert);
                        map[index] = -1;
                    }
                }
            }
        }
        TileIdMapSaveData save = new TileIdMapSaveData(width, height, bounds.xMin, bounds.yMin, map);
        UDebug.Print($"맵 ID 데이터를 생성했습니다. (성공한 개수 = {success})");
        return save;
    }

    // 딕셔너리를 받아서 Json 파일을 생성한다.
    private void ExportJson(TextAsset csv, Tilemap tileMap)
    {
        // 딕셔너리 생성 시도
        var fileToDict = BuildFileToDict(csv, tileMap);
        if (fileToDict == null)
        {
            UDebug.Print("File To Dict 생성 실패했습니다.", LogType.Assert);
            return;
        }
        // 타일 ID맵 생성 시도
        var save = BuildSaveData(fileToDict);
        if (save == null)
        {
            UDebug.Print("Tile ID Map 생성 실패했습니다.", LogType.Assert);
            return;
        }
        // Json 직렬화
        string json = JsonUtility.ToJson(save, true); // pretty : 줄바꿈 성실히
        File.WriteAllText($"{K.TILE_EXPORT_PATH}/{K.TILE_JSON_EXPORT_NAME}", json);
        AssetDatabase.Refresh();
        UDebug.Print($"타일 맵 데이터 직렬화 완료");
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnGUI()
    {
        GUILayout.Label("타일 CSV 출력 장치", EditorStyles.boldLabel); // 제목
        // 라벨, 오브젝트 변수, 타입 제한, 씬에서 드래그 허용
        _csvData = (TextAsset)EditorGUILayout.ObjectField("Tile CSV 파일", _csvData, typeof(TextAsset), false);
        _targetTileMap = (Tilemap)EditorGUILayout.ObjectField("등록할 타일맵", _targetTileMap, typeof(Tilemap), true);

        if (GUILayout.Button("타일맵 Json 생성")) // 버튼이 클릭된 순간 true
        {
            ExportJson(_csvData, _targetTileMap);
        }
    }
    #endregion
}
