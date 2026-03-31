using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TestSaveable : BaseMono, ISaveable
{
    private int _health = 100;

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

    [System.Serializable]
    private struct UnitSaveData
    {
        public int health;
        public Vector3 pos;
        public Quaternion rot;
    }
}
