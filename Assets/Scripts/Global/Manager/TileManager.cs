using System;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 모든 타일 맵, 상태를 미리 생성하여 담는 매니저
/// </summary>
public class TileManager : GlobalSingleton<TileManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private Tilemap _mainGraphicMap;
    private Tilemap _farmGraphicMap;
    private Tilemap _caveGraphicMap;
    private TileMap _mainLogicMap;
    private TileMap _farmLogicMap;
    private TileMap _caveLogicMap;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        BuildFullMap(ref _mainGraphicMap, K.TILE_RESOURCE_MAIN_BASE_PATH, ref _mainLogicMap, K.TILE_RESOURCE_MAIN_JSON_PATH);
        BuildFullMap(ref _farmGraphicMap, K.TILE_RESOURCE_FARM_BASE_PATH, ref _farmLogicMap, K.TILE_RESOURCE_FARM_JSON_PATH);
        BuildFullMap(ref _caveGraphicMap, K.TILE_RESOURCE_CAVE_BASE_PATH, ref _caveLogicMap, K.TILE_RESOURCE_CAVE_JSON_PATH);
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 초기화 진입점
    private void BuildFullMap(ref Tilemap graphicMap, string tileBasePath, ref TileMap logicMap, string tileJsonPath)
    {
        UDebug.Print($"그래픽 / 로직 타일맵 빌드를 시작합니다.");
        // finally 에서 정리하기 위해 미리 선언
        Tilemap tileBaseMap = null;
        TextAsset tileJson = null;
        try
        {
            // 준비해둔 리소스 모두 로드
            tileBaseMap = Resources.Load<Tilemap>(tileBasePath);
            tileJson = Resources.Load<TextAsset>(tileJsonPath);
            UDebug.Print($"모든 타일맵, Json을 리소스 폴더에서 로드했습니다.");
            // 유효성 검증
            if(!IsValid(tileBaseMap, tileJson, out TileIdMapSaveData idMap))
            {
                throw new Exception("유효성 검증 탈락");
            }
            //
            //TileSingle[] tiles = ;
            // 빌드 완료

            //
        }
        catch (Exception ex)
        {
            UDebug.Print("초기화 도중 예외가 발생했습니다.", LogType.Assert);
            UDebug.Print(ex.Message, LogType.Assert);
        }
        finally
        {
            // 리소스 해제
            Resources.UnloadAsset(tileBaseMap);
            Resources.UnloadAsset(tileJson);
        }
    }

    // 리소스 폴더에서 로드한 CSV를 읽어서 검증 후 문자열 생성
    private bool IsValid(Tilemap tile, TextAsset json, out TileIdMapSaveData idMap)
    {
        idMap = null;
        if (UDebug.IsNull(tile) || UDebug.IsNull(json))
        {
            return false;
        }
        // 시트가 비어있음
        string idText = json.text;
        if (string.IsNullOrEmpty(idText) || string.IsNullOrWhiteSpace(idText))
        {
            UDebug.Print($"Json이 비어있습니다.", LogType.Assert);
            return false;
        }
        // 타일맵 ID Json 맞나 확인
        idMap = JsonUtility.FromJson<TileIdMapSaveData>(idText);
        if(idMap == null)
        {
            UDebug.Print("유효한 Json 데이터가 아닙니다.", LogType.Assert);
            return false;
        }
        // 검증 완료
        return true;
    }

    // 타일맵 컴포넌트 빌드
    private void BuildGraphicMap(Tilemap graphicMap)
    {
        graphicMap.ClearAllTiles();
    }

    // TileMap 클래스 빌드
    private void BuildLogicMap(TileIdMapSaveData idMap)
    {
        // 변수 준비
        int width = idMap.width;
        int height = idMap.height;
        int length = width * height;
        TileSingle[] tiles = new TileSingle[length];
        int successCount = 0;
        // 타일맵 직사각형 순회
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // ID 및 좌표 가져오기 (오프셋 적용)
                int index = UGrid.GridToIndex(x, y, width);
                int tileID = idMap.tileIds[index];
                Vector3Int worldPos = new Vector3Int(idMap.startX + x, idMap.startY + y, 0);

                if (tileID != -1)
                {
                    /*// 그래픽 렌더링
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
                    }
                    else
                    {
                        tileData.state = ETileState.None;
                    }
                    _tileMap.SetTile(index, tileData);
                    successCount++;*/
                }
                // 빈 공간 처리
                else
                {
                    tiles[index].state = ETileState.None;
                }
            }
        }
        Debug.Log($"맵 빌드 완료. (크기: {idMap.width}x{idMap.height}, 유효 타일: {successCount}개)");
    }
    #endregion
}
