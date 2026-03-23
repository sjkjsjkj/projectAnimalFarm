using UnityEngine;

public class TInteract : BaseMono
{
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        // 마우스 좌클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            ProcessMouseClick();
        }
    }

    private void ProcessMouseClick()
    {
        // TileManager 초기화 및 현재 맵(Tile) 존재 여부 검증
        if (TileManager.Ins == null || TileManager.Ins.Tile == null)
        {
            Debug.LogWarning("[Tester] TileManager가 아직 초기화되지 않았거나 맵이 로드되지 않았습니다.");
            return;
        }

        // 마우스 스크린 좌표를 2D 월드 좌표(float)로 변환
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
        Vector2 worldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        TileMap map = TileManager.Ins.Tile;
        // 클릭한 좌표가 맵 바운더리 내부인지 검사
        if (!map.InMap(worldPos2D))
        {
            Debug.Log($"[Tester] 맵 외부 클릭 - WorldPos: ({worldPos2D.x:F2}, {worldPos2D.y:F2})");
            return;
        }

        // 월드 좌표를 1차원 배열 인덱스와 격자 좌표계로 변환
        int tileIndex = map.WorldToIndex(worldPos2D);
        Vector2Int gridPos = map.IndexToGrid(tileIndex);

        // 상태값
        bool isMoveable = map.IsMoveable(tileIndex);
        bool isFarmable = map.IsFarmable(tileIndex);
        bool isFishing = map.IsFishingable(tileIndex);
        bool isBuildable = map.IsBuildable(tileIndex);
        bool isInteractable = map.IsInteractable(tileIndex);
        bool isSeaFishing = map.IsSeaFishingable(tileIndex);

        Debug.Log($"[타일 정보] 월드 좌표: ({worldPos2D.x:F2}, {worldPos2D.y:F2}) | 격자 좌표: {gridPos} | 인덱스: {tileIndex}\n" +
                  $"▶ 이동: {isMoveable} | 경작: {isFarmable} | 낚시: {isFishing} | 건설: {isBuildable} | 파괴: {isSeaFishing} | 상호작용: {isInteractable}");
    }
}
