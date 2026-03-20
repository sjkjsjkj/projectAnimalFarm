using UnityEngine;

/// <summary>
/// 모든 데이터베이스를 총괄하는 브릿지 역할의 싱글턴 클래스 입니다.
/// Database.Ins.DB이름 으로 다른 DB에 접근할 수 있으며,
/// Database.Ins.DB.FindData(id)로 데이터를 불러올 수 있습니다.
/// </summary>
public class Database : GlobalSingleton<Database>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("Databases")]

    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private DatabaseSO<AnimalSO> _animalDB;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public DatabaseSO<AnimalSO> Animal => _animalDB;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }
        _animalDB = new DatabaseSO<AnimalSO>();

        _animalDB.MakeDB("ScriptableObject/Animal/");
       

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion
}
