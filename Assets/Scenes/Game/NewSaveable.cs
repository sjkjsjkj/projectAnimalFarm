/*using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class NewSaveable : BaseMono, ISaveable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("주제")]
    //[SerializeField] private Class _class;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private int _health = 100;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 매니저에서 사용할 유닛 ID
    public string UnitId { get; set; }

    // 매니저에서 호출하는 데이터 직렬화 함수
    public string SaveData()
    {
        UnitSaveData data = new();
        data.health = this._health;
        data.health1 = this._health1;
        data.health2 = this._health2;
        data.health3 = this._health3;
        data.health4 = this._health4;
        return JsonUtility.ToJson(data);
    }

    // 매니저에서 호출하는 데이터 복구 함수
    public void LoadData(string dataJson)
    {
        UnitSaveData data = JsonUtility.FromJson<UnitSaveData>(dataJson);
        this._health = data.health;
        this._health = data.health1;
        this._health = data.health2;
        this._health = data.health3;
        this._health = data.health4;
    }

    [System.Serializable]
    private struct UnitSaveData
    {
        public int health;
        public int health1;
        public int health2;
        public int health3;
        public int health4;
    }
}
*/
