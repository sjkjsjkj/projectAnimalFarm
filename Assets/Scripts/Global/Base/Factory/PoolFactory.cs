using UnityEngine;

/// <summary>
/// 풀링 객체를 생성하는 팩토리 클래스
/// </summary>
public class PoolFactory<T> where T : Component, IPoolable
{
    private ObjectPool<T> _pool;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 생성자
    public PoolFactory(ObjectPool<T> pool)
    {
        _pool = pool;
    }

    /// <summary>
    /// 풀링이 적용된 객체를 생성해서 반환합니다.
    /// </summary>
    public T Spawn()
    {
        if (_pool.Pull(out T instance))
        {
            instance.Initialize();
            instance.transform.SetParent(GameManager.ObjectRoot);
            return instance;
        }
        return null;
    }
    #endregion
}
