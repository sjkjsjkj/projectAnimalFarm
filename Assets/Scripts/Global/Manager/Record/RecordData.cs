using UnityEngine;

/// <summary>
/// 각종 데이터 저장
/// </summary>
[System.Serializable]
public class RecordData
{
    #region ─────────────────────────▶ 공개 변수 ◀─────────────────────────
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
    #endregion
}
