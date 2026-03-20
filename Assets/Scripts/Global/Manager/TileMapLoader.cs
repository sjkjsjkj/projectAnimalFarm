using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

/// <summary>
/// 인스펙터에 등록된 타일 맵으로 빌드를 시작합니다.
/// </summary>
public class TileMapLoader : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("타일맵 컴포넌트")]
    [SerializeField] private Tilemap _tileMapGraphic;

    [Header("Json Data")]
    [SerializeField] private TextAsset _jsonMapData;
    #endregion

    private TileMap _tileMap; // 논리 충돌 및 상태 담당
    // 런타임에 ID로 타일 에셋과 상태를 빠르게 찾기 위한 캐시
    private Dictionary<int, TileBase> _idToVisualTile = new();
    private Dictionary<int, int> _idToStateFlag = new();

    private void Start()
    {
        BuildTileDatabase(); // 에셋 및 상태 캐싱
        LoadMapFromJson(); // 맵 구축
    }

    // ID → 상태 + 타일맵 빌드
    private void BuildTileDatabase()
    {
        UDebug.Print("[TileMap Loader] 타일 데이터베이스 빌드를 완료했습니다.");
    }

    //
    private void LoadMapFromJson()
    {
        // 방어코드
        if (_jsonMapData == null || _tileMapGraphic == null)
        {
            UDebug.Print("[TileMap Loader] Json 파일 또는 타일맵이 누락되었습니다.", LogType.Assert);
            return;
        }
        // Json → 구조체 역 직렬화
        TileIdMapSaveData save = JsonUtility.FromJson<TileIdMapSaveData>(_jsonMapData.text);
        _tileMap = new TileMap(save.width, save.height);
        _tileMapGraphic.ClearAllTiles();

        int successCount = 0;
        int width = save.width;
        int height = save.height;
        // 타일맵 직사각형 순회
        for (int y = 0; y < save.height; y++)
        {
            for (int x = 0; x < save.width; x++)
            {
                int index = UGrid.GridToIndex(x, y, save.width);
                int tileID = save.tiles[index];
                // 오프셋 적용
                Vector3Int worldPos = new Vector3Int(save.startX + x, save.startY + y, 0);
                if (tileID != -1)
                {
                    // 그래픽 렌더링
                    if (_idToVisualTile.TryGetValue(tileID, out TileBase visualTile))
                    {
                        _tileMapGraphic.SetTile(worldPos, visualTile);
                    }
                    else
                    {
                        UDebug.Print("딕셔너리에 등록된 TileBase가 없습니다.", LogType.Assert);
                    }
                    // 상태 부여
                    TileSingle tileData = new TileSingle();
                    if (_idToStateFlag.TryGetValue(tileID, out int stateFlag))
                    {
                        tileData.state = (ETileState)stateFlag;
                    } else
                    {
                        tileData.state = ETileState.None;
                    }
                    _tileMap.SetTile(index, tileData);
                    successCount++;
                }
                // 빈 공간 처리
                else
                {
                    TileSingle tileData = new TileSingle();
                    tileData.state = ETileState.None;
                    _tileMap.SetTile(index, tileData);
                }
            }
        }
        Debug.Log($"[MapLoader] 맵 로드 완료. (크기: {save.width}x{save.height}, 유효 타일: {successCount}개)");
    }
}
