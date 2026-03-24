using UnityEngine;

/// <summary>
/// SO데이터베이스를 구축하기 위해 사용되는 SO의 베이스 형태입니다.
/// </summary>
public class DatabaseUnitSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] private string _id = "";
    [SerializeField] private Sprite _image;
    [SerializeField] private string _name = "이름";
    [SerializeField] private string _description = "설명";
    [SerializeField] private GameObject _prefab;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string Id => _id;
    public Sprite Image => _image;
    public string Name => _name;
    public string Description => _description;
    public GameObject Prefab => _prefab;
    // 값 유효성 검사
    public virtual bool IsValid()
    {
        if (string.IsNullOrEmpty(_id))
        {
            return false;
        }
        if (string.IsNullOrEmpty(_name))
        {
            return false;
        }
        if (string.IsNullOrEmpty(_description))
        {
            return false;
        }
        return true;
    }
    #endregion

    private void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"DatabaseUnitSO SO({_id})의 값이 올바르지 않습니다.", LogType.Assert);
        }
    }
}
