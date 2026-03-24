using UnityEngine;

/// <summary>
/// 모든 SO 클래스가 상속받아야하는 기본 데이터입니다.
/// </summary>
public abstract class DatabaseUnitSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] protected string _id = "";
    [SerializeField] protected EType _type = EType.None;
    [SerializeField] protected string _name = "이름";
    [SerializeField] protected string _description = "설명";
    [SerializeField, CsvIgnore] protected Sprite _image;
    [SerializeField, CsvIgnore] protected GameObject _prefab;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string Id => _id;
    public EType Type => _type;
    public string Name => _name;
    public string Description => _description;
    public Sprite Image => _image;
    public GameObject Prefab => _prefab;

    // 정상 값을 가지는지 검사
    public virtual bool IsValid()
    {
        if (string.IsNullOrEmpty(_id)) return false;
        if (string.IsNullOrWhiteSpace(_id)) return false;
        if (string.IsNullOrEmpty(_name)) return false;
        if (string.IsNullOrWhiteSpace(_name)) return false;
        if (string.IsNullOrEmpty(_description)) return false;
        if (string.IsNullOrWhiteSpace(_description)) return false;
        if (_image == null) return false;
        if (_prefab == null) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    // 인스펙터 변수 유효성 검사
    protected virtual void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO({_id})의 값이 올바르지 않습니다.", LogType.Assert);
        }
    }
    #endregion
}
