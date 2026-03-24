using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO_", menuName = "Item/Base", order = 1)]
public class ItemSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] private string _id;
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _name = "이름";
    [SerializeField] private string _description = "설명";
    [SerializeField] private int _maxStack = 64; // 최대 중첩 수
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    //public string Id => _id;
    public Sprite Icon => _icon;
    public string Name => _name;
    public string Description => _description;
    public int MaxStack => _maxStack;

    public virtual bool IsValid()
    {
        if (string.IsNullOrEmpty(_id)) return false;
        if (string.IsNullOrEmpty(_name)) return false;
        if (string.IsNullOrEmpty(_description)) return false;
        if (_maxStack <= 0) return false;
        return true;
    }
    #endregion

    private void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"아이템 SO({_id})의 값이 올바르지 않습니다.", LogType.Assert);
        }
    }
}
