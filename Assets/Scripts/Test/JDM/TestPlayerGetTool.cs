using UnityEngine;

/// <summary>
/// 키 코드 1~5로 플레이어 애니메이션 재생
/// </summary>
public class TestPlayerGetTool : Frameable
{
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        if(DatabaseManager.Ins == null || InventoryManager.Ins == null)
        {
            return;
        }
        if (!InventoryManager.Ins.IsSettingFinish)
        {
            return;
        }
        var data = DatabaseManager.Ins;
        var inventory = InventoryManager.Ins.PlayerInventory;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var pickaxe = data.Item(Id.Item_Tool_BasicPickAxe);
            inventory.TryGetItem(pickaxe);
            var fishing = data.Item(Id.Item_Tool_BasicFishingRod);
            inventory.TryGetItem(fishing);
            var watering = data.Item(Id.Item_Tool_BasicWateringCan);
            inventory.TryGetItem(watering);
            var sickle = data.Item(Id.Item_Tool_BasicSickle);
            inventory.TryGetItem(sickle);
            var shovel = data.Item(Id.Item_Tool_BasicShovel);
            inventory.TryGetItem(shovel);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            var pickaxe = data.Item(Id.Item_Tool_SolidPickAxe);
            inventory.TryGetItem(pickaxe);
            var fishing = data.Item(Id.Item_Tool_SolidFishingRod);
            inventory.TryGetItem(fishing);
            var watering = data.Item(Id.Item_Tool_SolidWateringCan);
            inventory.TryGetItem(watering);
            var sickle = data.Item(Id.Item_Tool_SolidSickle);
            inventory.TryGetItem(sickle);
            var shovel = data.Item(Id.Item_Tool_SolidShovel);
            inventory.TryGetItem(shovel);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            var pickaxe = data.Item(Id.Item_Tool_SuperPickAxe);
            inventory.TryGetItem(pickaxe);
            var fishing = data.Item(Id.Item_Tool_SuperFishingRod);
            inventory.TryGetItem(fishing);
            var watering = data.Item(Id.Item_Tool_SuperWateringCan);
            inventory.TryGetItem(watering);
            var sickle = data.Item(Id.Item_Tool_SuperSickle);
            inventory.TryGetItem(sickle);
            var shovel = data.Item(Id.Item_Tool_SuperShovel);
            inventory.TryGetItem(shovel);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            var pickaxe = data.Item(Id.Item_Tool_PrimePickAxe);
            inventory.TryGetItem(pickaxe);
            var fishing = data.Item(Id.Item_Tool_PrimeFishingRod);
            inventory.TryGetItem(fishing);
            var watering = data.Item(Id.Item_Tool_PrimeWateringCan);
            inventory.TryGetItem(watering);
            var sickle = data.Item(Id.Item_Tool_PrimeSickle);
            inventory.TryGetItem(sickle);
            var shovel = data.Item(Id.Item_Tool_PrimeShovel);
            inventory.TryGetItem(shovel);
        }
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            var pickaxe = data.Item(Id.Item_Tool_MasterPickAxe);
            inventory.TryGetItem(pickaxe);
            var fishing = data.Item(Id.Item_Tool_MasterFishingRod);
            inventory.TryGetItem(fishing);
            var watering = data.Item(Id.Item_Tool_MasterWateringCan);
            inventory.TryGetItem(watering);
            var sickle = data.Item(Id.Item_Tool_MasterSickle);
            inventory.TryGetItem(sickle);
            var shovel = data.Item(Id.Item_Tool_MasterShovel);
            inventory.TryGetItem(shovel);
        }
    }
}
