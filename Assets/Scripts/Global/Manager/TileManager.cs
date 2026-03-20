/// <summary>
/// 모든 타일 맵을 미리 생성하여 담는 매니저
/// </summary>
public class TileManager : GlobalSingleton<TileManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    private readonly TileMap _mainMap;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion
}
