using UnityEngine;

/// <summary>
/// 씬 로드 시 플레이어 시작 지점
/// </summary>
public class SceneLoadStartPoint : BaseMono
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    [SerializeField] private Transform _defaultSpawnPoints;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private Transform _playerTr;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void SetPlayerPosition(Transform pointTr)
    {
        Vector2 pos = pointTr.position;
        SetPlayerPosition(pos);
    }
    private void SetPlayerPosition(Vector2 pos)
    {
        var player = DataManager.Ins.Player;
        _playerTr.position = pos;
        player.SetTransform(pos, Vector2.down);
    }
    private void PlayerMovedSpawnpoint()
    {
        var player = DataManager.Ins.Player;
        var game = GameManager.Ins;
        int spawnIndex = game.NextSpawnPointIndex;
        // 게임 내에서 맵 이동
        if (spawnIndex != -1)
        {
            if (_spawnPoints == null)
            {
                UDebug.Print($"스폰 포인트가 비어있습니다.", LogType.Error);
                return;
            }
            int length = _spawnPoints.Length;
            if (length < 0 || length <= spawnIndex)
            {
                UDebug.Print($"스폰 포인트 인덱스가 범위를 초과했습니다. spawnIndex: {spawnIndex}, spawnPoints.Length: {length}", LogType.Error);
                return;
            }
            if (_spawnPoints[spawnIndex] == null)
            {
                UDebug.Print($"스폰 포인트가 비어있습니다. spawnIndex: {spawnIndex}", LogType.Error);
                return;
            }
            SetPlayerPosition(_spawnPoints[spawnIndex]);
            game.NextSpawnPointIndex = -1;
            UDebug.Print($"씬 로드 : {spawnIndex}번 스폰 포인트에 플레이어 등장");
        }
        else // 게임 로드 (시작)
        {
            Vector2 pos = (player.Position == Vector2.zero) ? _defaultSpawnPoints.position : player.Position;
            UDebug.Print($"씬 로드 : 초기 위치에서 플레이어 등장" +
                $"\n게임 시작 위치: {pos}, 플레이어 : {player.Position}");
            UDebug.Print($"");
            SetPlayerPosition(pos);
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Start()
    {
        PlayerMovedSpawnpoint();
    }
    #endregion
}
