using UnityEngine;

/// <summary>
/// 세이브/로드 테스트용 오브젝트
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class TestSaveObj : Frameable, ISaveable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _changeDirInterval = 5f;
    #endregion

    private float _nextDirChangeTime;
    private Vector2 _moveDir;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override EPriority Priority => EPriority.Lv5;
    public override void ExecuteFrame()
    {
        if (_nextDirChangeTime < Time.time)
        {
            SetRandomDirection();
            _nextDirChangeTime = Time.time + _changeDirInterval;
        }
        // 이동
        float movement = (_moveSpeed * Time.deltaTime);
        transform.Translate(_moveDir * movement, Space.World);
    }

    // 매니저에서 사용할 유닛 ID
    public string UnitId { get; set; } = Id.World_Animal_TestObj;

    // 매니저에서 호출하는 데이터 직렬화 함수
    public string SaveData()
    {
        UnitSaveData data = new();
        data.pos = this.transform.position;
        data.rot = this.transform.rotation;
        return JsonUtility.ToJson(data);
    }

    // 매니저에서 호출하는 데이터 복구 함수
    public void LoadData(string dataJson)
    {
        UnitSaveData data = JsonUtility.FromJson<UnitSaveData>(dataJson);
        this.transform.position = data.pos;
        this.transform.rotation = data.rot;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void SetRandomDirection()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float x = Mathf.Cos(randomAngle);
        float y = Mathf.Sin(randomAngle);
        _moveDir = new Vector2(x, y).normalized;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        _nextDirChangeTime = Time.time;
        SetRandomDirection();
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    [System.Serializable]
    private struct UnitSaveData
    {
        public Vector3 pos;
        public Quaternion rot;
    }
    #endregion
}
