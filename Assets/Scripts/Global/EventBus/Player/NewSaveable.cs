using UnityEngine;

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
    // 매니저에서 호출하는 데이터 직렬화 함수
    public string SaveData()
    {
        UnitSaveData data = new();
        data.health = _health;
        data.pos = transform.position;
        data.rot = transform.rotation;
        return JsonUtility.ToJson(data);
    }

    // 매니저에서 호출하는 데이터 복구 함수
    public void LoadData(string dataJson)
    {
        UnitSaveData data = JsonUtility.FromJson<UnitSaveData>(dataJson);
        _health = data.health;
        transform.position = data.pos;
        transform.rotation = data.rot;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    [System.Serializable]
    private struct UnitSaveData
    {
        public int health;
        public Vector3 pos;
        public Quaternion rot;
    }
    #endregion
}
