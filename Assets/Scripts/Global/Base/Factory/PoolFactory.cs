
using UnityEngine;

/// <summary>
/// 풀을 사용하는 팩토리입니다.
/// </summary>
public class PoolFactory
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private ObjectPool _pool;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶  생성자   ◀─────────────────────────
    public PoolFactory(ObjectPool pool)
    {
        _pool = pool;
    }
    #endregion
    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public PoolItem Spawn()
    {
        PoolItem tempGo = _pool.Get();
        tempGo.SetInfo();
        return tempGo;
        //DatabaseUnitSO tempSO = _animalDB.FindData(id);
        //return MakeGo(tempSO);
    }
    //private PoolItem MakeGo(DatabaseUnitSO data)
    //{
        
    //}
    #endregion
}
