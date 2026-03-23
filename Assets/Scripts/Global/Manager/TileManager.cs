using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 모든 타일 맵, 상태를 미리 생성하여 담는 매니저
/// </summary>
public class TileManager : GlobalSingleton<TileManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private TileMap _mainLogicMap;
    private TileMap _farmLogicMap;
    private TileMap _caveLogicMap;
    private TileMap _curLogicMap;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public TileMap Tile => _curLogicMap;

    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        BuildLogicMap(ref _mainLogicMap, K.TILE_RESOURCE_MAIN_JSON_PATH);
        BuildLogicMap(ref _farmLogicMap, K.TILE_RESOURCE_FARM_JSON_PATH);
        BuildLogicMap(ref _caveLogicMap, K.TILE_RESOURCE_CAVE_JSON_PATH);
        EventBus<OnSceneChanged>.Subscribe(MapChangeHandle); // OnEnable이 아닌 이곳에서 구독
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void MapChangeHandle(OnSceneChanged ctx)
    {
        UDebug.Print($"타일 로직 맵을 {_curLogicMap}에서 {ctx.scene}으로 교체합니다.");
        switch (ctx.scene)
        {
            case EScene.Main:
                if(_mainLogicMap == null)
                {
                    UDebug.Print("메인 로직 맵이 초기화되지 않은 상태에서 호출되었습니다.", LogType.Assert);
                    return;
                }
                _curLogicMap = _mainLogicMap;
                break;
            case EScene.Farm:
                if (_farmLogicMap == null)
                {
                    UDebug.Print("농사 로직 맵이 초기화되지 않은 상태에서 호출되었습니다.", LogType.Assert);
                    return;
                }
                _curLogicMap = _farmLogicMap;
                break;
            case EScene.Cave:
                if (_caveLogicMap == null)
                {
                    UDebug.Print("동굴 로직 맵이 초기화되지 않은 상태에서 호출되었습니다.", LogType.Assert);
                    return;
                }
                _curLogicMap = _caveLogicMap;
                break;
        }
    }

    // 초기화 진입점
    private void BuildLogicMap(ref TileMap logicMap, string tileJsonPath)
    {
        UDebug.Print($"그래픽 / 로직 타일맵 빌드를 시작합니다.");
        // finally 에서 정리하기 위해 미리 선언
        TextAsset tileJson = null;
        try
        {
            // 준비해둔 리소스 모두 로드
            tileJson = Resources.Load<TextAsset>(tileJsonPath);
            UDebug.Print($"Json을 리소스 폴더에서 로드 시도했습니다.");
            // 유효성 검증
            if (!IsValid(tileJson, out TileIdMapSaveData idMap))
            {
                throw new Exception("Json 유효성 검증 탈락");
            }
            // TileMap 작성
            logicMap = BuildTileMapData(idMap);
        }
        catch (Exception ex)
        {
            UDebug.Print("초기화 도중 예외가 발생했습니다.", LogType.Assert);
            UDebug.Print(ex.Message, LogType.Assert);
        }
        finally
        {
            // 리소스 해제
            if (tileJson != null)
            {
                Resources.UnloadAsset(tileJson);
            }
        }
    }

    // 리소스 폴더에서 로드한 CSV를 읽어서 검증 후 문자열 생성
    private bool IsValid(TextAsset json, out TileIdMapSaveData idMap)
    {
        idMap = null;
        if (UDebug.IsNull(json))
        {
            UDebug.Print($"Json을 리소스 폴더에서 찾을 수 없습니다.", LogType.Assert);
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
        if (idMap == null)
        {
            UDebug.Print("유효한 Json 데이터가 아닙니다.", LogType.Assert);
            return false;
        }
        // 검증 완료
        return true;
    }

    // TileMap 클래스 빌드
    private TileMap BuildTileMapData(TileIdMapSaveData idMap)
    {
        // 변수 준비
        int width = idMap.width;
        int height = idMap.height;
        int length = width * height;
        TileSingle[] tiles = new TileSingle[length];
        int successCount = 0;
        // 타일맵 직사각형 순회
        for (int i = 0; i < length; ++i)
        {
            int tileID = idMap.tileIds[i];
            // 어떤 타일이 존재함
            if (tileID != -1)
            {
                tiles[i].id = tileID;
                tiles[i].state = TileIdToState.Ins[tileID]; // ID에 맞는 타일 상태;
                successCount++;
            }
            // 비어있는 타일
            else
            {
                tiles[i].id = -1;
                tiles[i].state = ETileState.None;
            }
        }
        Debug.Log($"맵 빌드 완료. (크기: {idMap.width}x{idMap.height}, 유효 타일: {successCount}개)");
        return new TileMap(tiles, width, height, idMap.startX, idMap.startY);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnDisable()
    {
        EventBus<OnSceneChanged>.Unsubscribe(MapChangeHandle);
    }
    #endregion
}
