using UnityEngine;

/// <summary>
/// 풀을 사용하지 않는 일반 팩토리 입니다.
/// </summary>
public class BasicFactory<T> where T : InfoObject
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private GameObject _prefab;
    private DatabaseSO<AnimalSO> _database;
    #endregion

    #region ─────────────────────────▶  생성자  ◀─────────────────────────
    public BasicFactory(GameObject prefab, DatabaseSO<AnimalSO> db)
    {
        _prefab = prefab;
        _database = db;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public GameObject Spawn(string id)
    {
        DatabaseUnitSO tempSO = _database.FindData(id);
        return MakeGo(tempSO);
    }
    private GameObject MakeGo(DatabaseUnitSO data)
    {
        GameObject tempGo = Object.Instantiate(_prefab);
        tempGo.GetComponent<T>().SetInfo(data);

        return tempGo;
    }
    #endregion
}
