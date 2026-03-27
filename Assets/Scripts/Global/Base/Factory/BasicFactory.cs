using UnityEngine;

/// <summary>
/// 풀을 사용하지 않는 일반 팩토리 입니다.
/// </summary>
public class BasicFactory<T> where T : InfoObject
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private DatabaseSO<AnimalWorldSO> _database;
    #endregion

    #region ─────────────────────────▶  생성자  ◀─────────────────────────
    public BasicFactory(DatabaseSO<AnimalWorldSO> db)
    {
        _database = db;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public GameObject Spawn(string id)
    {
        UnitSO tempSO = _database.FindData(id);
        return MakeGo(tempSO);
    }
    private GameObject MakeGo(UnitSO data)
    {
        GameObject tempGo = Object.Instantiate(data.Prefab);
        tempGo.GetComponent<T>().SetInfo(data);

        return tempGo;
    }
    #endregion
}
