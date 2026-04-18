using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인벤토리를 저장하기 위한 래퍼 클래스
/// </summary>
[System.Serializable]
public class SavedInventoryList
{
    [SerializeField] public List<SavedInventoryData> inventorys = new();
}
