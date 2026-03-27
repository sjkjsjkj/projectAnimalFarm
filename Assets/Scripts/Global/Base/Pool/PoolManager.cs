using UnityEngine;

/// <summary>
/// 오브젝트 풀을 관리하는 매니저 (허브 역할)
/// </summary>
public class PoolManager : Singleton<PoolManager>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("프리팹")]
    [SerializeField] private TestVFX _vfxPrefab;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private ObjectPool<TestVFX> _vfxPool;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public ObjectPool<TestVFX> VFX => _vfxPool;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }
        _vfxPool = new ObjectPool<TestVFX>(_vfxPrefab, false, 50);
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion
}
