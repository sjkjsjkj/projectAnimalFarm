using UnityEngine;

/// <summary>
/// 싱글톤 클래스의 설계 의도입니다.
/// </summary>
public class Database : Singleton<Database>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("주제")]
    //[SerializeField] private Class _class;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    [SerializeField] private ConsumableItemDB _consumeItemDB;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public ConsumableItemDB Consume => _consumeItemDB;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }
        _consumeItemDB.MakeDB("https://docs.google.com/spreadsheets/d/12dSRC2d-tfEtGsveerWi7wymgL4xyFlt1byC9EE0-to/export?format=tsv&range=A2:B");

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion
}
