using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "HarvestSO_", menuName = "HarvestSO", order = 1)]
public class HarvestSO : DatabaseUnitSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] private EHarvest _idEnum = EHarvest.None;
    [SerializeField] private float _grownTime;
    //[SerializeField] private Sprite[] _harvestSprite;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    //public string Id => _idEnum.ToString();
    public float GrownTime => _grownTime;
    //public Sprite[] Sprites => _harvestSprite;

    // 값 유효성 검사
    public override bool IsValid()
    {
        base.IsValid();
        if (_idEnum == EHarvest.None)
        {
            return false;
        }
        if (_grownTime == 0f)
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
            UDebug.PrintOnce($"HarvestSO SO({_idEnum})의 값이 올바르지 않습니다.", LogType.Assert);
        }
    }
}
