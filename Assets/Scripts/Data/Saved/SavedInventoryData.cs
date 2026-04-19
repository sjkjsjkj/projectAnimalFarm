using UnityEngine;

/// <summary>
/// 인벤토리를 저장하기 위한 클래스
/// </summary>
[System.Serializable]
public class SavedInventoryData
{
    [SerializeField] public EInventoryType type;
    [SerializeField] public SavedSlotData[] slots;
}
