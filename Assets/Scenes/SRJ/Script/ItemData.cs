using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string description;
    public ItemType itemType;
    public int maxStack = 99; // 추가

    public enum ItemType { Consumable, Equipment, Etc }
}
