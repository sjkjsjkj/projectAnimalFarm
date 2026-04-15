using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각종 데이터 저장
/// </summary>
[System.Serializable]
public class RecordData
{
    #region ─────────────────────────▶ 직렬화 변수 ◀─────────────────────────
    // 현재 목표
    [SerializeField] public int goalIndex;
    // 상호작용 횟수
    [SerializeField] public int totalOreMinedCount; // 광물 캐기
    [SerializeField] public int totalFishCaughtCount; // 낚시
    [SerializeField] public int totalDrinkingCount;
    [SerializeField] public int totalEatingCount;
    [SerializeField] public int totalGatheringCount; // 채집

    [SerializeField] public int totalPlowCount; // 경작
    [SerializeField] public int totalPlantingCount; // 심기
    [SerializeField] public int totalWateringCount; // 물주기
    [SerializeField] public int totalHarvestingCount; // 수확
    // 이동 거리      
    [SerializeField] public float totalWalkingDistance; // 걷기
    [SerializeField] public float totalRunningDistance; // 뛰기
    // Ui
    [SerializeField] public int totalInventoryOpenCount; // 인벤토리 열기
    [SerializeField] public int totalCraftingOpenCount; // 제작 열기
    [SerializeField] public int totalPictorialOpenCount; // 도감 열기
    [SerializeField] public int totalChestOpenCount; // 창고 열기
    [SerializeField] public int totalFeedBoxOpenCount; // 먹이통 열기
    // 아이템 획득 정보
    [SerializeField] private List<string> _itemId = new(); // 딕셔너리의 키 (직렬화)
    [SerializeField] private List<int> _itemAmount = new(); // 딕셔너리의 값 (직렬화)
    [SerializeField] private List<EType> _typeId = new(); // 딕셔너리의 키 (직렬화)
    [SerializeField] private List<int> _typeAmount = new(); // 딕셔너리의 값 (직렬화)
    #endregion

    private Dictionary<string, int> _itemRecord = new(); // 아이템 런타임 데이터
    private Dictionary<EType, int> _typeRecord = new(); // 아이템 런타임 데이터

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    /// <summary>
    /// 특정 아이템의 획득량을 기록합니다.
    /// </summary>
    public void AddItemRecord(string itemId, int amount)
    {
        if (_itemRecord == null)
        {
            _itemRecord = new Dictionary<string, int>();
        }
        if (_typeRecord == null)
        {
            _typeRecord = new Dictionary<EType, int>();
        }
        if (_itemRecord.ContainsKey(itemId))
        {
            _itemRecord[itemId] += amount;
        }
        else
        {
            _itemRecord[itemId] = amount;
        }
        // 타입
        UnitSO so = DatabaseManager.Ins.Unit(itemId);
        if(so == null)
        {
            UDebug.Print($"아이디 {itemId}에 해당하는 UnitSO를 찾을 수 없습니다.", LogType.Error);
            return;
        }
        EType type = so.Type;
        if (_typeRecord.ContainsKey(type))
        {
            _typeRecord[type] += amount;
        }
        else
        {
            _typeRecord[type] = amount;
        }
    }

    /// <summary>
    /// 특정 아이템 데이터 가져오기
    /// </summary>
    public int GetItemRecord(string itemId)
    {
        if (_itemRecord != null && _itemRecord.TryGetValue(itemId, out int amount))
        {
            return amount;
        }
        return 0;
    }

    /// <summary>
    /// 특정 아이템 데이터 가져오기
    /// </summary>
    public int GetTypeRecord(EType type)
    {
        if (_typeRecord != null && _typeRecord.TryGetValue(type, out int amount))
        {
            return amount;
        }
        return 0;
    }

    /// <summary>
    /// 세이브 전에 호출
    /// </summary>
    public void SaveBeforeSerialize()
    {
        // 아이템을 리스트로 변환
        if (_itemId == null)
        {
            _itemId = new List<string>();
        }
        if (_itemAmount == null)
        {
            _itemAmount = new List<int>();
        }
        _itemId.Clear();
        _itemAmount.Clear();
        if (_itemRecord != null)
        {
            foreach (var kvp in _itemRecord)
            {
                _itemId.Add(kvp.Key);
                _itemAmount.Add(kvp.Value);
            }
        }
        // 타입을 리스트로 변환
        if(_typeId == null)
        {
            _typeId = new List<EType>();
        }
        if(_typeAmount == null)
        {
            _typeAmount = new List<int>();
        }
        _typeId.Clear();
        _typeAmount.Clear();
        if(_typeRecord != null)
        {
            foreach(var kvp in _typeRecord)
            {
                _typeId.Add(kvp.Key);
                _typeAmount.Add(kvp.Value);
            }
        }
    }

    /// <summary>
    /// 로드 후 호출
    /// </summary>
    public void LoadAfterDeserialize()
    {
        // 역직렬화 후에 리스트를 딕셔너리로 변환
        if (_itemRecord == null)
        {
            _itemRecord = new Dictionary<string, int>();
        }
        _itemRecord.Clear();
        int length = Mathf.Min(_itemId.Count, _itemAmount.Count);
        for (int i = 0; i < length; i++)
        {
            _itemRecord[_itemId[i]] = _itemAmount[i];
        }
        // Type도 적용
        if(_typeRecord == null)
        {
            _typeRecord = new Dictionary<EType, int>();
        }
        _typeRecord.Clear();
        int typeLength = Mathf.Min(_typeId.Count, _typeAmount.Count);
        for(int i = 0; i < typeLength; i++)
        {
            _typeRecord[_typeId[i]] = _typeAmount[i];
        }
    }
    #endregion
}
