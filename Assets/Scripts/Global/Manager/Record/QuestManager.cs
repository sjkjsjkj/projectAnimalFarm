using UnityEngine;

/// <summary>
/// 싱글톤 클래스의 설계 의도입니다.
/// </summary>
public class QuestManager : GlobalSingleton<QuestManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }

    public void CheckQuestProgress()
    {
        var record = DataManager.Ins.Record;
        int length = QuestContainer.list.Count;
        // 모든 퀘스트 클리어
        if (record.goalIndex >= length)
        {
            return;
        }
        // 현재 퀘스트
        var curQuest = QuestContainer.list[record.goalIndex];
        // 조건 검사
        if (curQuest.IsComplete(record))
        {
            var player = DataManager.Ins.Player;
            // 사운드 발생
            USound.PlaySfx(Id.Sfx_Other_Success_1, player.Position);
            // 보상 지급
            player.AddMoney(curQuest.RewardGold);
            if (!string.IsNullOrEmpty(curQuest.RewardItemId))
            {
                var inventory = InventoryManager.Ins.PlayerInventory;
                ItemSO so = DatabaseManager.Ins.Item(curQuest.RewardItemId);
                inventory.TryGetItem(so, curQuest.RewardItemAmount);
            }
            // 다음 퀘스트로
            UDebug.Print($"퀘스트 클리어: {curQuest.Description}");
            record.goalIndex++;
            string title = record.goalIndex >= length ? null : QuestContainer.list[record.goalIndex].Description;
            OnQuestChanged.Publish(record.goalIndex, title);
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 핸들 ◀─────────────────────────
    private void PlayerMinedHandle(OnPlayerMined ctx)
    {
        DataManager.Ins.Record.totalOreMinedCount++;
    }
    private void PlayerFishCaughthandle(OnPlayerFishCaught ctx)
    {
        DataManager.Ins.Record.totalFishCaughtCount++;
    }
    private void PlayerDrinkingHandle(OnPlayerDrinking ctx)
    {
        DataManager.Ins.Record.totalDrinkingCount++;
    }
    private void PlayerEatingHandle(OnPlayerEating ctx)
    {
        DataManager.Ins.Record.totalEatingCount++;
    }
    private void PlayerCollectedHandle(OnPlayerCollected ctx)
    {
        DataManager.Ins.Record.totalGatheringCount++;
    }
    private void PlayerPlowHandle(OnPlayerPlow ctx)
    {
        DataManager.Ins.Record.totalPlowCount++;
    }
    private void PlayerPlantingSeedsHandle(OnPlayerPlantingSeeds ctx)
    {
        DataManager.Ins.Record.totalPlantingCount++;
    }
    private void PlayerWateringHandle(OnPlayerWatering ctx)
    {
        DataManager.Ins.Record.totalWateringCount++;
    }
    private void PlayerHarvestingHandle(OnPlayerHarvesting ctx)
    {
        DataManager.Ins.Record.totalHarvestingCount++;
    }
    private void PlayerWalkingHandle(OnPlayerWalking ctx)
    {
        DataManager.Ins.Record.totalWalkingDistance += ctx.movement;
        UDebug.Print($"총 걷기 거리: {DataManager.Ins.Record.totalWalkingDistance}");
    }
    private void PlayerRunningHandle(OnPlayerRunning ctx)
    {
        DataManager.Ins.Record.totalRunningDistance += ctx.movement;
    }
    private void PlayerInventoryOpenHandle(OnInventoryOpen ctx)
    {
        DataManager.Ins.Record.totalInventoryOpenCount++;
    }
    private void PlayerCraftingOpenHandle(OnCraftingOpen ctx)
    {
        DataManager.Ins.Record.totalCraftingOpenCount++;
    }
    private void PlayerPictorialOpenHandle(OnPictorialOpen ctx)
    {
        DataManager.Ins.Record.totalPictorialOpenCount++;
    }
    private void PlayerChestOpenHandle(OnChestOpen ctx)
    {
        DataManager.Ins.Record.totalChestOpenCount++;
    }
    private void PlayerGetItemHandle(OnPlayerGetItem ctx)
    {
        DataManager.Ins.Record.AddItemRecord(ctx.itemId, ctx.amount);
    }
    private void PlayerFeedBoxOpenHandle(OnFeedBoxOpen ctx)
    {
        DataManager.Ins.Record.totalFeedBoxOpenCount++;
    }
    private void PlayerBuyAnimalHandle(OnBuyAnimal ctx)
    {
        DataManager.Ins.Record.AddItemRecord(ctx.animalId, 1);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    // 구독
    private void OnEnable()
    {
        // 상호작용
        EventBus<OnPlayerMined>.Subscribe(PlayerMinedHandle);
        EventBus<OnPlayerFishCaught>.Subscribe(PlayerFishCaughthandle);
        EventBus<OnPlayerDrinking>.Subscribe(PlayerDrinkingHandle);
        EventBus<OnPlayerEating>.Subscribe(PlayerEatingHandle);
        EventBus<OnPlayerCollected>.Subscribe(PlayerCollectedHandle);
        // 농사
        EventBus<OnPlayerPlow>.Subscribe(PlayerPlowHandle);
        EventBus<OnPlayerPlantingSeeds>.Subscribe(PlayerPlantingSeedsHandle);
        EventBus<OnPlayerWatering>.Subscribe(PlayerWateringHandle);
        EventBus<OnPlayerHarvesting>.Subscribe(PlayerHarvestingHandle);
        // 이동 거리
        EventBus<OnPlayerWalking>.Subscribe(PlayerWalkingHandle);
        EventBus<OnPlayerRunning>.Subscribe(PlayerRunningHandle);
        // Ui
        EventBus<OnInventoryOpen>.Subscribe(PlayerInventoryOpenHandle);
        EventBus<OnCraftingOpen>.Subscribe(PlayerCraftingOpenHandle);
        EventBus<OnPictorialOpen>.Subscribe(PlayerPictorialOpenHandle);
        EventBus<OnChestOpen>.Subscribe(PlayerChestOpenHandle);
        EventBus<OnPlayerGetItem>.Subscribe(PlayerGetItemHandle);
        EventBus<OnFeedBoxOpen>.Subscribe(PlayerFeedBoxOpenHandle);
        EventBus<OnBuyAnimal>.Subscribe(PlayerBuyAnimalHandle);
    }

    // 해제
    private void OnDisable()
    {
        // 상호작용
        EventBus<OnPlayerMined>.Unsubscribe(PlayerMinedHandle);
        EventBus<OnPlayerFishCaught>.Unsubscribe(PlayerFishCaughthandle);
        EventBus<OnPlayerDrinking>.Unsubscribe(PlayerDrinkingHandle);
        EventBus<OnPlayerEating>.Unsubscribe(PlayerEatingHandle);
        EventBus<OnPlayerCollected>.Unsubscribe(PlayerCollectedHandle);
        // 농사
        EventBus<OnPlayerPlow>.Unsubscribe(PlayerPlowHandle);
        EventBus<OnPlayerPlantingSeeds>.Unsubscribe(PlayerPlantingSeedsHandle);
        EventBus<OnPlayerWatering>.Unsubscribe(PlayerWateringHandle);
        EventBus<OnPlayerHarvesting>.Unsubscribe(PlayerHarvestingHandle);
        // 이동 거리
        EventBus<OnPlayerWalking>.Unsubscribe(PlayerWalkingHandle);
        EventBus<OnPlayerRunning>.Unsubscribe(PlayerRunningHandle);
        // Ui
        EventBus<OnInventoryOpen>.Unsubscribe(PlayerInventoryOpenHandle);
        EventBus<OnCraftingOpen>.Unsubscribe(PlayerCraftingOpenHandle);
        EventBus<OnPictorialOpen>.Unsubscribe(PlayerPictorialOpenHandle);
        EventBus<OnChestOpen>.Unsubscribe(PlayerChestOpenHandle);
        EventBus<OnPlayerGetItem>.Unsubscribe(PlayerGetItemHandle);
        EventBus<OnFeedBoxOpen>.Unsubscribe(PlayerFeedBoxOpenHandle);
        EventBus<OnBuyAnimal>.Unsubscribe(PlayerBuyAnimalHandle);
    }
    #endregion
}
