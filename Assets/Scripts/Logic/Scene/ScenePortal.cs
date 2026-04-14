using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class ScenePortal : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("주제")]
    [SerializeField] private EScene _nextScene;
    [SerializeField] private int _entryIndex; // 다음 씬에서 플레이어가 나타날 위치 인덱스
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnTriggerEnter2D(Collider2D col)
    {
        // 플레이어
        if (!col.CompareTag("Player"))
        {
            return;
        }
        // 다음 씬 이동
        var game = GameManager.Ins;
        game.LoadSceneAsyncWithFade((int)_nextScene);
        game.NextSpawnPointIndex = _entryIndex;
    }
    #endregion
}
