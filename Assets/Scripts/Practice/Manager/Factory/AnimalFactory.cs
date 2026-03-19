using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class AnimalFactory : FactoryUnit
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public GameObject Prefab => _prefab;
    #endregion


    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────


    public override GameObject Spawn(string id)
    {
        GameObject tempGo = Instantiate(_prefab);

        UDebug.IsNull(tempGo.GetComponent<AnimalObject>());

        tempGo.GetComponent<AnimalObject>().SetInfo(Database.Ins.Animal.FindData(id));

        return tempGo;
    }
    #endregion
}
