using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "DatabaseUnitSO_", menuName = "DatabaseUnitSO", order = 1)]
public class DatabaseUnitSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] private string _id = "";
    [SerializeField] private Sprite _image;
    [SerializeField] private string _name = "이름";
    [SerializeField] private string _description = "설명";
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string Id => _id;
    public Sprite Image => _image;
    public string Name => _name;
    public string Description => _description;

    // 값 유효성 검사
    public virtual bool IsVaild()
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
        if (!IsVaild())
        {
            UDebug.PrintOnce($"DatabaseUnitSO SO({_id})의 값이 올바르지 않습니다.", LogType.Assert);
        }
    }
}
