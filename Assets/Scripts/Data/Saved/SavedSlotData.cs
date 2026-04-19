using UnityEngine;

/// <summary>
/// 인벤토리 슬롯을 저장하기 위한 구조체
/// </summary>
[System.Serializable]
public struct SavedSlotData
{
    [SerializeField] public string itemId;
    [SerializeField] public int amount;
}
