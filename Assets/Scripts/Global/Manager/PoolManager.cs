using UnityEngine;

/// <summary>
/// 오브젝트 풀들을 관리하는 매니저입니다.
/// 다른 오브젝트들에 접근하는 허브의 역할을 하고 있습니다.
/// </summary>
public class PoolManager : Singleton<PoolManager>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("프리팹")]
    [SerializeField] private TestVFX _vfxPrefab;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private ObjectPool _vfxPool;

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public ObjectPool VFX => _vfxPool;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }
        _vfxPool = new ObjectPool(50, _vfxPrefab);
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion
}
