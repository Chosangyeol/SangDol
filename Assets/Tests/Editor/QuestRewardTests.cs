using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class QuestRewardRig : IDisposable
{
    public readonly InventoryTransferRig InventoryRig;
    public readonly QuestManager Quest;
    public readonly ItemManager Items;
    public readonly NormalItemSO Other;
    private readonly GameObject questObject, itemObject;
    private readonly QuestManager previousQuest = QuestManager.Instance;
    private readonly ItemManager previousItems = ItemManager.Instance;
    private readonly Action previousLevel = GameEvent.OnPlayerLevelUp;
    public GameObject Root => questObject;
    public C_Inventory Inventory => InventoryRig.Inventory;
    public CharacterModel Model => InventoryRig.Model;
    public QuestRewardRig(int capacity = 4)
    {
        InventoryRig = new InventoryTransferRig(capacity); GameEvent.OnPlayerLevelUp = null;
        questObject = new GameObject("QuestRewardTest"); questObject.SetActive(false);
        Quest = questObject.AddComponent<QuestManager>();
        InventoryTransferRig.Set(Quest, "_model", Model); QuestManager.Instance = Quest;
        itemObject = new GameObject("QuestRewardItems"); itemObject.SetActive(false);
        Items = itemObject.AddComponent<ItemManager>(); ItemManager.Instance = Items;
        var normal = new Dictionary<string, ItemBaseSO>(); var gear = new Dictionary<string, EquipItemSO>();
        foreach (var guid in AssetDatabase.FindAssets("t:ItemDataBaseSO"))
        {
            var db = AssetDatabase.LoadAssetAtPath<ItemDataBaseSO>(AssetDatabase.GUIDToAssetPath(guid));
            foreach (var item in db.itemDataBase) if (item != null) normal[item.itemID] = item;
            foreach (var item in db.equipItemDataBase) if (item != null) gear[item.itemID] = item;
        }
        InventoryRig.Material.itemID = "20002"; normal["20002"] = InventoryRig.Material;
        Other = ScriptableObject.CreateInstance<NormalItemSO>();
        Other.itemID = "20003"; Other.maxStack = 10; Other.stackable = true; normal["20003"] = Other;
        InventoryTransferRig.Set(Items, "itemBaseDic", normal); InventoryTransferRig.Set(Items, "equipItemDic", gear);
        typeof(QuestManager).GetMethod("BindInventory", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Quest, null);
    }
    public QuestData Seed(params RewardItem[] rewards)
    {
        var data = new QuestData { questID = "test-quest", questName = "Reward test", questType = "Kill",
            rewardGold = 20, rewardExp = 10, rewardItems = new List<RewardItem>(rewards) };
        Quest.questDict[data.questID] = data; Quest.questStateDict[data.questID] = QuestState.CanClear;
        return data;
    }
    public RewardItem Reward(string id, int count) => new RewardItem { itemID = id, count = count };
    public QuestData Csv(string id)
    {
        typeof(QuestManager).GetMethod("LoadQuestCsv", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Quest, new object[] { "NpcQuestDataBase" });
        var data = Quest.questDict[id]; Quest.questStateDict[id] = QuestState.CanClear; return data;
    }
    public string Snapshot()
    {
        var stat = Model.Stat.Stat; string result = stat.gold + "/" + stat.currentLevel + "/" + stat.currentExp + "/" + stat.statPoint + "/" + stat.remainSkillPoint + "/" + stat.totalSkillPoint;
        foreach (var item in Inventory.Items) result += item == null ? "/empty" : "/" + item.itemBaseSO.itemID + ":" + item.currentStack;
        return result;
    }
    public void Dispose()
    {
        if (Quest != null) typeof(QuestManager).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Quest, null);
        UnityEngine.Object.DestroyImmediate(questObject); UnityEngine.Object.DestroyImmediate(itemObject);
        UnityEngine.Object.DestroyImmediate(Other);
        QuestManager.Instance = previousQuest; ItemManager.Instance = previousItems;
        GameEvent.OnPlayerLevelUp = previousLevel; InventoryRig.Dispose();
    }
}
public static class QuestRewardCases
{
    public static void CsvAppleAwardsAllRewards()
    {
        using (var r = new QuestRewardRig())
        {
            var data = r.Csv("Q_30001_002");
            var apple = r.Items.GetItemBaseSO(data.questTarget); Assert.IsNotNull(apple);
            r.Inventory.AddItem(apple.CreateItem(1)); int gold = r.Model.Stat.Stat.gold;
            r.Quest.CompleteQuest(data.questID);
            Assert.AreEqual(20, r.Inventory.GetTotalItemCount("20001"));
            Assert.AreEqual(gold + 200, r.Model.Stat.Stat.gold); Assert.AreEqual(3, r.Model.Stat.Stat.currentLevel);
            Assert.AreEqual(0, r.Model.Stat.Stat.currentExp); Assert.AreEqual(QuestState.Completed, r.Quest.GetQuestState(data.questID));
            Assert.AreEqual(1, r.Inventory.GetTotalItemCount(data.questTarget), "Reward fix preserves existing non-consumption policy.");
        }
    }
    public static void CsvWolfAwardsGearGoldAndExp()
    {
        using (var r = new QuestRewardRig())
        {
            var data = r.Csv("Q_30001_003"); int gold = r.Model.Stat.Stat.gold;
            r.Quest.CompleteQuest(data.questID);
            Assert.AreEqual(1, r.Inventory.GetTotalItemCount("10008"));
            Assert.IsInstanceOf<EquipItemBase>(r.Inventory.Items[0]); Assert.AreEqual(gold + 500, r.Model.Stat.Stat.gold);
            Assert.AreEqual(5, r.Model.Stat.Stat.currentLevel); Assert.AreEqual(0, r.Model.Stat.Stat.currentExp);
            Assert.AreEqual(QuestState.Completed, r.Quest.GetQuestState(data.questID));
        }
    }
    public static void InsufficientBatchSpaceIsAtomicAndRetryable()
    {
        using (var r = new QuestRewardRig(1))
        {
            var old = new NormalItemBase(r.InventoryRig.Material, 8); r.Inventory.SetItemAt(0, old);
            var data = r.Seed(r.Reward("20002", 2), r.Reward("20003", 1)); string before = r.Snapshot();
            r.Quest.CompleteQuest(data.questID); Assert.AreEqual(before, r.Snapshot()); Assert.AreSame(old, r.Inventory.Items[0]);
            Assert.AreEqual(QuestState.CanClear, r.Quest.GetQuestState(data.questID));
            data.rewardItems.RemoveAt(1); r.Quest.CompleteQuest(data.questID);
            Assert.AreEqual(10, old.currentStack); Assert.AreEqual(QuestState.Completed, r.Quest.GetQuestState(data.questID));
        }
    }
    public static void DuplicateRewardsMergeAndPreserveExistingReference()
    {
        using (var r = new QuestRewardRig(2))
        {
            var old = new NormalItemBase(r.InventoryRig.Material, 5); r.Inventory.SetItemAt(0, old);
            var data = r.Seed(r.Reward("20002", 4), r.Reward("20002", 8)); r.Quest.CompleteQuest(data.questID);
            Assert.AreSame(old, r.Inventory.Items[0]); Assert.AreEqual(10, old.currentStack);
            Assert.AreEqual(7, r.Inventory.Items[1].currentStack); Assert.AreEqual(17, r.Inventory.GetTotalItemCount("20002"));
        }
    }
    public static void MultipleEquipmentRewardsRemainDistinctUnits()
    {
        using (var r = new QuestRewardRig(3))
        {
            var data = r.Seed(r.Reward("10008", 3)); r.Quest.CompleteQuest(data.questID);
            Assert.AreEqual(3, r.Inventory.GetTotalItemCount("10008"));
            Assert.AreNotSame(r.Inventory.Items[0], r.Inventory.Items[1]);
            foreach (var item in r.Inventory.Items) { Assert.IsInstanceOf<EquipItemBase>(item); Assert.AreEqual(1, item.currentStack); }
        }
    }
    public static void InvalidSecondRewardDoesNotPartiallyPay()
    {
        using (var r = new QuestRewardRig())
        {
            var data = r.Seed(r.Reward("20002", 3), r.Reward("99999", 1)); string before = r.Snapshot();
            r.Quest.CompleteQuest(data.questID); Assert.AreEqual(before, r.Snapshot());
            Assert.AreEqual(QuestState.CanClear, r.Quest.GetQuestState(data.questID));
        }
    }
    public static void BadInputsAndOversizedEquipmentFailWithoutMutation()
    {
        using (var r = new QuestRewardRig(2))
        {
            Assert.DoesNotThrow(() => { r.Quest.CompleteQuest(null); r.Quest.CompleteQuest(" "); r.Quest.CompleteQuest("missing"); });
            foreach (int amount in new[] { 0, -1, int.MaxValue })
            {
                var data = r.Seed(r.Reward("10008", amount)); string before = r.Snapshot();
                r.Quest.CompleteQuest(data.questID); Assert.AreEqual(before, r.Snapshot());
                Assert.AreEqual(QuestState.CanClear, r.Quest.GetQuestState(data.questID));
            }
        }
    }
    public static void CurrencyOnlyWorksWithoutItemManager()
    {
        using (var r = new QuestRewardRig())
        {
            var data = r.Seed(); ItemManager.Instance = null; int gold = r.Model.Stat.Stat.gold;
            r.Quest.CompleteQuest(data.questID); Assert.AreEqual(gold + 20, r.Model.Stat.Stat.gold);
            Assert.AreEqual(QuestState.Completed, r.Quest.GetQuestState(data.questID));
        }
    }
    public static void GoldOverflowDoesNotPartiallyPay()
    {
        using (var r = new QuestRewardRig())
        {
            var data = r.Seed(r.Reward("20002", 1)); r.Model.Stat.Stat.gold = int.MaxValue;
            string before = r.Snapshot(); r.Quest.CompleteQuest(data.questID);
            Assert.AreEqual(before, r.Snapshot()); Assert.AreEqual(QuestState.CanClear, r.Quest.GetQuestState(data.questID));
        }
    }
    public static void RewardNotificationsObserveFullCommitAndCannotReenter()
    {
        using (var r = new QuestRewardRig(2))
        {
            var data = r.Seed(r.Reward("20002", 3), r.Reward("20003", 2)); int gold = r.Model.Stat.Stat.gold;
            int notifications = 0; GameEvent.OnGetItem = _ =>
            {
                notifications++; Assert.AreEqual(QuestState.Completed, r.Quest.GetQuestState(data.questID));
                Assert.AreEqual(3, r.Inventory.GetTotalItemCount("20002")); Assert.AreEqual(2, r.Inventory.GetTotalItemCount("20003"));
                Assert.AreEqual(gold + 20, r.Model.Stat.Stat.gold); Assert.AreEqual(10, r.Model.Stat.Stat.currentExp); r.Quest.CompleteQuest(data.questID);
            };
            r.Inventory.OnInventoryUpdated += () => r.Quest.CompleteQuest(data.questID);
            r.Quest.CompleteQuest(data.questID); string after = r.Snapshot(); r.Quest.CompleteQuest(data.questID);
            Assert.AreEqual(after, r.Snapshot()); Assert.AreEqual(2, notifications);
        }
    }
    public static void StackMergeAndRemovalUpdateQuestState()
    {
        using (var r = new QuestRewardRig(1))
        {
            var data = r.Seed(); data.questType = "Item"; data.questTarget = "20002"; data.questCount = 5;
            var old = new NormalItemBase(r.InventoryRig.Material, 3); r.Inventory.SetItemAt(0, old);
            Assert.AreEqual(QuestState.InProgress, r.Quest.GetQuestState(data.questID)); int acquired = 0;
            GameEvent.OnGetItem = _ => { acquired++; Assert.AreEqual(5, r.Inventory.GetTotalItemCount("20002")); };
            r.Inventory.AddItem(new NormalItemBase(r.InventoryRig.Material, 2));
            Assert.AreEqual(1, acquired); Assert.AreSame(old, r.Inventory.Items[0]);
            Assert.AreEqual(QuestState.CanClear, r.Quest.GetQuestState(data.questID));
            r.Inventory.RemoveTargetItem("20002", 1); Assert.AreEqual(QuestState.InProgress, r.Quest.GetQuestState(data.questID));
            int gold = r.Model.Stat.Stat.gold; r.Quest.CompleteQuest(data.questID); Assert.AreEqual(gold, r.Model.Stat.Stat.gold);
            old.currentStack = 5; r.Quest.questStateDict[data.questID] = QuestState.CanClear;
            r.Quest.CompleteQuest(data.questID); Assert.AreEqual(QuestState.Completed, r.Quest.GetQuestState(data.questID));
            r.Inventory.RemoveItem(old); Assert.AreEqual(QuestState.Completed, r.Quest.GetQuestState(data.questID));
        }
    }
    public static void NewSlotAcquisitionNotificationsObserveFinalQuantity()
    {
        foreach (int existing in new[] { 0, 5 })
        {
            using (var r = new QuestRewardRig(3))
            {
                var old = existing == 0 ? null : new NormalItemBase(r.InventoryRig.Material, existing);
                if (old != null) r.Inventory.SetItemAt(0, old);
                int acquired = 0, inserted = 0;
                GameEvent.OnGetItem = _ => { acquired++; Assert.AreEqual(25 + existing, r.Inventory.GetTotalItemCount("20002")); };
                r.Inventory.OnAddItemInventory += _ => { inserted++; Assert.AreEqual(25 + existing, r.Inventory.GetTotalItemCount("20002")); };
                var donor = new NormalItemBase(r.InventoryRig.Material, 25);
                r.Inventory.AddItem(donor);
                Assert.AreEqual(0, donor.currentStack);
                Assert.AreEqual(existing == 0 ? 3 : 2, acquired);
                Assert.AreEqual(acquired, inserted);
                if (old != null) Assert.AreSame(old, r.Inventory.Items[0]);
            }
        }
    }
    public static void StaleCollectionReadyStateIsRechecked()
    {
        using (var r = new QuestRewardRig())
        {
            var data = r.Seed(r.Reward("20003", 1)); data.questType = "Item"; data.questTarget = "20002"; data.questCount = 1;
            string before = r.Snapshot(); r.Quest.CompleteQuest(data.questID);
            Assert.AreEqual(before, r.Snapshot()); Assert.AreEqual(QuestState.InProgress, r.Quest.GetQuestState(data.questID));
        }
    }
}
public static class QuestRewardRuntimeCases
{
    public static IEnumerator QuestLifecycleRefreshesOnEnableAndUnsubscribesOnDestroy()
    {
        using (var movement = new MovementRig(true))
        using (var r = new QuestRewardRig())
        {
            QuestManager.Instance = null;
            InventoryTransferRig.Set(r.Quest, "_model", movement.Model);
            r.Root.SetActive(true); yield return null;
            var data = r.Seed();
            data.questType = "Item"; data.questTarget = "20002"; data.questCount = 5;
            var inventory = movement.Model.Inventory;
            inventory.AddItem(new NormalItemBase(r.InventoryRig.Material, 5));
            Assert.AreEqual(QuestState.CanClear, r.Quest.GetQuestState(data.questID));
            r.Root.SetActive(false); inventory.RemoveTargetItem("20002", 1);
            Assert.AreEqual(QuestState.CanClear, r.Quest.GetQuestState(data.questID));
            r.Root.SetActive(true);
            Assert.AreEqual(QuestState.InProgress, r.Quest.GetQuestState(data.questID));
            Assert.AreEqual(4, r.Quest.questItemProgressDict[data.questID]);
            r.Root.SetActive(false); r.Root.SetActive(true);
            int progress = 0; r.Quest.OnQuestProgressUpdated += () => progress++;
            inventory.RemoveTargetItem("20002", 1); Assert.AreEqual(1, progress);
            UnityEngine.Object.DestroyImmediate(r.Root); Assert.IsTrue(QuestManager.Instance == null);
            inventory.RemoveTargetItem("20002", 1); Assert.AreEqual(1, progress);
        }
    }

    public static IEnumerator RewardNotificationsObserveFullCommitAndCannotReenter()
    {
        QuestRewardCases.RewardNotificationsObserveFullCommitAndCannotReenter(); yield return null;
    }
    public static IEnumerator StackMergeAndRemovalUpdateQuestState()
    {
        QuestRewardCases.StackMergeAndRemovalUpdateQuestState(); yield return null;
    }
}