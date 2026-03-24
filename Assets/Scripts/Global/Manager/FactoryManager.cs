using UnityEngine;

/// <summary>
/// 모든 팩토리 총괄하는 브릿지 역할의 싱글턴 클래스 입니다.
/// Factory.Ins.DB이름 으로 다른 DB에 접근할 수 있으며,
/// Factory.Ins.DB.Spawn(id)으로 객체를 불러올 수 있습니다.
/// </summary>
public class FactoryManager : Singleton<FactoryManager>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private BasicFactory<AnimalObject> _animalFactory;
    private PoolFactory _vfxFactory;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public BasicFactory<AnimalObject> Animal => _animalFactory;
    public PoolFactory VFX => _vfxFactory;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }

        _animalFactory = new BasicFactory<AnimalObject>(DatabaseManager.Ins.Animal);
        _vfxFactory = new PoolFactory(PoolManager.Ins.VFX);
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion
}
