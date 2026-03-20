using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class BasicFactory<TComponent> where TComponent : InfoObject
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
        tempGo.GetComponent<TComponent>().SetInfo(data);

        return tempGo;
    }
    #endregion
}
