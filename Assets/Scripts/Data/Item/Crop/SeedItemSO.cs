using UnityEngine;

/// <summary>
/// 씨앗 아이템이 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "SeedItemSO_", menuName = "ScriptableObjects/Item/Seed", order = 1)]
public class SeedItemSO : ItemSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("씨앗 아이템 정보")]
    [Tooltip("심을 작물 ID")]
    [SerializeField] protected string _placeCropId;
    [Tooltip("심기 위해 필요한 농사 레벨")]
    [SerializeField] protected int _needFarmingLevel = 1;
    [SerializeField] protected string _harvestItemId;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string PlaceCropId => _placeCropId;
    public int NeedFarmingLevel => _needFarmingLevel;
    public string HarvestItemId => _harvestItemId;

    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_type != EType.SeedItem) return false;
        if (_placeCropId.IsEmpty()) return false;
        if (_needFarmingLevel < 1) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    // 인스펙터 변수 유효성 검사
    protected override void OnValidate()
    {
        base.OnValidate();
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {_id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
