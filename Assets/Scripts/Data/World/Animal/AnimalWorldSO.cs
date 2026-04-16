using UnityEngine;

/// <summary>
/// 동물이 월드에서 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "AnimalWorldSO_", menuName = "ScriptableObjects/World/Animal", order = 1)]
public class AnimalWorldSO : WorldSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("애니메이션")]
    [SerializeField, CsvIgnore] protected RuntimeAnimatorController _animController; // 동물 애니메이션

    [Header("먹이 정보")]
    [SerializeField] protected bool _needFood = true; // 밥이 필요한 동물?
    [SerializeField] protected ERarity _feedMinRarity = ERarity.Basic; // 먹는 밥의 등급
    [SerializeField] protected float _tickFeedAmount = 0.1f; // 1틱당 소모하는 만복도
    [SerializeField] protected float _maxFeedAmount = 100; // 최대 만복도

    [Header("생산품 정보")]
    [SerializeField] protected string _productId = null; // 생산하는 아이템 ID
    [SerializeField] protected int _productMinCount = 1; // 한 번에 생산하는 아이템 최소 개수
    [SerializeField] protected int _productMaxCount = 1; // 한 번에 생산하는 아이템 최대 개수
    [SerializeField] protected int _productTime = 5; // 생산에 걸리는 틱 수
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public RuntimeAnimatorController AnimController => _animController;
    public bool NeedFood => _needFood;
    public ERarity FeedRarity => _feedMinRarity;
    public float TickFeedAmount => _tickFeedAmount;
    public float MaxFeedAmount => _maxFeedAmount;
    public string ProductId => _productId;
    public int ProductMinCount => _productMinCount;
    public int ProductMaxCount => _productMaxCount;
    public int ProductTime => _productTime;


    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_type != EType.AnimalWorld) return false;
        if (_animController == null) return false;
        // 먹이
        if (_feedMinRarity == ERarity.None) return false;
        if (_tickFeedAmount < 0f) return false;
        if (_maxFeedAmount < 0f) return false;
        // 생산품
        if (_productId.IsEmpty()) return false;
        if (ProductMinCount <= 0) return false;
        if (ProductMaxCount <= 0) return false;
        if (ProductTime <= 0f) return false;
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
