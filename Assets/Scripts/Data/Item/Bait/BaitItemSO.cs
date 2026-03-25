using UnityEngine;

/// <summary>
/// 미끼 아이템이 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "BaitItemSO_", menuName = "ScriptableObjects/Item/Bait", order = 1)]
public class BaitItemSO : ItemSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("미끼 아이템 정보")]
    [SerializeField] protected ERarity[] _catchFishRarity; // 낚을 수 있는 물고기 등급
    [SerializeField] protected float[] _catchFishWeight; // 각 등급이 낚이는 가중치
    [SerializeField] protected bool _canUseSea; // 바다에 사용 가능한 미끼?
    [SerializeField, HideInInspector] private float _totalWeight; // 가중치 총합 캐싱
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public ERarity CatchFishRarity(int index) => _catchFishRarity[index];
    public float CatchFishWeight(int index) => _catchFishWeight[index];
    public bool CanUseSea => _canUseSea;

    /// <summary>
    /// 특정 등급의 물고기가 낚일 확률을 반환합니다.
    /// </summary>
    public float GetFishChance(int index)
    {
        if (index < 0 || index >= _catchFishWeight.Length)
        {
            return 0f;
        }
        return _catchFishWeight[index] / _totalWeight;
    }

    /// <summary>
    /// 내부에서 가중치를 고려하여 계산해서 물고기 등급을 반환합니다.
    /// </summary>
    public ERarity GetFishRarity()
    {
        if(_totalWeight <= 0f)
        {
            return ERarity.None;
        }
        float targetWeight = Random.Range(0f, _totalWeight);
        int length = _catchFishRarity.Length;
        for (int i = 0; i < length; ++i)
        {
            targetWeight -= _catchFishWeight[i];
            if(targetWeight <= 0f)
            {
                return _catchFishRarity[i];
            }
        }
        return _catchFishRarity[length - 1];
    }

    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_type != EType.SeedItem) return false;
        if (!UArray.IsInitedArray(_catchFishRarity)) return false;
        if (!UArray.IsInitedArray(_catchFishWeight)) return false;
        int length = _catchFishRarity.Length;
        if (length != _catchFishWeight.Length) return false;
        for (int i = 0; i < length; ++i)
        {
            if (_catchFishRarity[i] == ERarity.None) return false;
            if (_catchFishWeight[i] <= 0) return false;
        }
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 가중치 총합 계산
    private void CalcTotalWeight()
    {
        _totalWeight = 0f;
        int length = _catchFishRarity.Length;
        for (int i = 0; i < length; ++i)
        {
            _totalWeight += _catchFishWeight[i];
        }
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
        else
        {
            CalcTotalWeight();
        }
    }
    #endregion
}
