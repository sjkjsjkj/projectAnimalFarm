using UnityEngine;

/// <summary>
/// 월드에서 성장하는 작물이 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "CropWorldSO_", menuName = "ScriptableObjects/World/Crop", order = 1)]
public class CropWorldSO : WorldSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("농사 정보")]
    [SerializeField] private float[] _growTimes; // 단계별 성장 필요 시간
    [SerializeField] private Sprite[] _stageSprites; // 단계별 스프라이트
    [SerializeField] private int _needFarmingLevel = 1; // 요구 농사 레벨
    [SerializeField] private ERarity _needWateringCanRarity = ERarity.Basic; // 요구 물뿌리개 레벨

    [Header("수확 정보")]
    [SerializeField] private string _harvestItemId; // 수확 아이템 ID
    [SerializeField] private int _minHarvestAmount = 1; // 수확 시 최소 획득량
    [SerializeField] private int _maxHarvestAmount = 1; // 수확 시 최대 획득량
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public float GrowTime(int index) => _growTimes[index];
    public Sprite StageSprites(int index) => _stageSprites[index];
    public int NeedFarmingLevel => _needFarmingLevel;
    public ERarity NeedWateringCanLevel => _needWateringCanRarity;
    public string HarvestItemId => _harvestItemId;
    public int MinHarvestAmount => _minHarvestAmount;
    public int MaxHarvestAmount => _maxHarvestAmount;

    /// <summary>
    /// 수확 가능한 상태인지 반환합니다.
    /// </summary>
    /// <param name="stage">현재 성장 단계</param>
    /// <returns></returns>
    public bool CanHarvest(int stage) => (stage >= _growTimes.Length);

    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_type != EType.CropWorld) return false;
        if (!UArray.IsInitedArray(_growTimes)) return false;
        if (!UArray.IsInitedArray(_stageSprites)) return false;
        int length = _growTimes.Length;
        if (length != _stageSprites.Length) return false;
        for (int i = 0; i < length; ++i)
        {
            if (_growTimes[i] <= 0f) return false;
        }
        if (_needFarmingLevel < 1) return false;
        if (_needWateringCanRarity == ERarity.None) return false;
        if (_harvestItemId.IsEmpty()) return false;
        if (_minHarvestAmount <= 0) return false;
        if (_maxHarvestAmount <= 0) return false;
        if (_minHarvestAmount > _maxHarvestAmount) return false;
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
            UDebug.PrintOnce($"SO({_id})의 값이 올바르지 않습니다.", LogType.Assert);
        }
    }
    #endregion
}
