using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Assembly-CSharp types are accessed by the test asmdefs through the existing Editor bridge pattern.
public sealed class InventoryTransferRig : IDisposable
{
    public readonly CharacterModel Model;
    public readonly C_Inventory Inventory;
    public readonly C_Equipment Equipment;
    public readonly NormalItemSO Material;
    private readonly GameObject actor;
    private readonly List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
    private readonly UIManager previousUI = UIManager.Instance;
    private readonly Action<string> previousGetItem = GameEvent.OnGetItem;
    private readonly Action<CharacterStat> previousStat = GameEvent.OnStatChange;

    public InventoryTransferRig(int capacity = 2)
    {
        typeof(UIManager).GetProperty("Instance").SetValue(null, null);
        GameEvent.OnGetItem = null;
        GameEvent.OnStatChange = null;
        actor = new GameObject("InventoryTransferActor");
        actor.SetActive(false);
        var agent = actor.AddComponent<NavMeshAgent>();
        agent.enabled = false;
        Model = actor.AddComponent<CharacterModel>();
        Model.enabled = false;
        var stats = ScriptableObject.CreateInstance<CharacterStatSO>();
        stats.attackDamage = 100f; stats.maxHp = 100f;
        assets.Add(stats);
        Set(Model, "navMesh", agent);
        Set(Model, "stat", new C_Stat(Model, stats));
        Inventory = new C_Inventory(Model, capacity);
        Equipment = new C_Equipment(Model);
        Set(Model, "inventory", Inventory);
        Set(Model, "equipment", Equipment);
        Material = ScriptableObject.CreateInstance<NormalItemSO>();
        Material.itemID = "transfer-material"; Material.itemName = "Test material";
        Material.stackable = true; Material.maxStack = 10;
        assets.Add(Material);
    }

    public static void Set(object target, string name, object value)
        => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    public EquipItemBase Gear(int upgrade = 1, ItemEnums.EquipItemType type = ItemEnums.EquipItemType.Weapon)
    {
        var data = ScriptableObject.CreateInstance<EquipItemSO>();
        data.itemID = "transfer-gear"; data.itemName = "Test gear";
        data.stackable = false; data.maxStack = 1; data.equipItemType = type;
        data.statToIncrease = C_Enums.CharacterStat.AttackDamage;
        data.value = 10f; data.perUpgradeBonus = 5f;
        assets.Add(data);
        return new EquipItemBase(data, 1) { currentUpgradeLevel = upgrade };
    }
    public float Attack => Model.Stat.Stat.attackDamage.FinalValue;
    public int TotalUnits
    {
        get
        {
            int count = 0;
            foreach (var item in Inventory.Items) if (item != null) count += item.currentStack;
            foreach (var item in Equipment.equipItems.Values) if (item != null) count += item.currentStack;
            return count;
        }
    }
    public EquipItemBase EquipFirst()
    {
        var item = Gear(); Inventory.SetItemAt(0, item); Equipment.EquipItem(item); return item;
    }
    public void Dispose()
    {
        UnityEngine.Object.DestroyImmediate(actor);
        foreach (var asset in assets) UnityEngine.Object.DestroyImmediate(asset);
        typeof(UIManager).GetProperty("Instance").SetValue(null, previousUI);
        GameEvent.OnGetItem = previousGetItem;
        GameEvent.OnStatChange = previousStat;
    }
}

public static class InventoryTransferCases
{
    public static void FullInventoryUnequipPreservesEverything()
    {
        using (var r = new InventoryTransferRig())
        {
            var gear = r.EquipFirst();
            var a = new NormalItemBase(r.Material, 3); var b = new NormalItemBase(r.Material, 4);
            r.Inventory.SetItemAt(0, a); r.Inventory.SetItemAt(1, b);
            int notifications = 0; r.Inventory.OnInventoryUpdated += () => notifications++;
            r.Equipment.UnequipItem(ItemEnums.EquipItemType.Weapon);
            Assert.AreSame(gear, r.Equipment.equipItems[ItemEnums.EquipItemType.Weapon]);
            Assert.AreSame(a, r.Inventory.Items[0]); Assert.AreSame(b, r.Inventory.Items[1]);
            Assert.AreEqual(115f, r.Attack); Assert.AreEqual(8, r.TotalUnits);
            Assert.AreEqual(1, gear.currentUpgradeLevel); Assert.AreEqual(0, notifications);
        }
    }
    public static void FullInventoryGearSwapPreservesReferencesAndStats()
    {
        using (var r = new InventoryTransferRig())
        {
            var old = r.EquipFirst(); var next = r.Gear(4);
            var other = new NormalItemBase(r.Material, 7);
            r.Inventory.SetItemAt(0, next); r.Inventory.SetItemAt(1, other);
            r.Equipment.EquipItem(next);
            Assert.AreSame(next, r.Equipment.equipItems[ItemEnums.EquipItemType.Weapon]);
            Assert.AreSame(old, r.Inventory.Items[0]); Assert.AreSame(other, r.Inventory.Items[1]);
            Assert.AreEqual(130f, r.Attack); Assert.AreEqual(9, r.TotalUnits);
            Assert.AreEqual(1, old.currentUpgradeLevel); Assert.AreEqual(4, next.currentUpgradeLevel);
        }
    }
    public static void UnequipToOccupiedMaterialIsRejected()
    {
        using (var r = new InventoryTransferRig())
        {
            var gear = r.EquipFirst(); var material = new NormalItemBase(r.Material, 5);
            r.Inventory.SetItemAt(0, material);
            r.Equipment.UnequipItem(ItemEnums.EquipItemType.Weapon, 0);
            Assert.AreSame(material, r.Inventory.Items[0]);
            Assert.AreSame(gear, r.Equipment.equipItems[ItemEnums.EquipItemType.Weapon]);
            Assert.AreEqual(115f, r.Attack); Assert.AreEqual(6, r.TotalUnits);
        }
    }
    public static void UnequipToDifferentGearTypeIsRejected()
    {
        using (var r = new InventoryTransferRig())
        {
            var gear = r.EquipFirst(); var head = r.Gear(3, ItemEnums.EquipItemType.Head);
            r.Inventory.SetItemAt(0, head);
            r.Equipment.UnequipItem(ItemEnums.EquipItemType.Weapon, 0);
            Assert.AreSame(head, r.Inventory.Items[0]);
            Assert.AreSame(gear, r.Equipment.equipItems[ItemEnums.EquipItemType.Weapon]);
            Assert.AreEqual(115f, r.Attack); Assert.AreEqual(2, r.TotalUnits);
        }
    }
    public static void UnequipToSameTypeSwapsInPlace()
    {
        using (var r = new InventoryTransferRig())
        {
            var old = r.EquipFirst(); var next = r.Gear(4); r.Inventory.SetItemAt(1, next);
            r.Equipment.UnequipItem(ItemEnums.EquipItemType.Weapon, 1);
            Assert.AreSame(old, r.Inventory.Items[1]);
            Assert.AreSame(next, r.Equipment.equipItems[ItemEnums.EquipItemType.Weapon]);
            Assert.AreEqual(130f, r.Attack); Assert.AreEqual(2, r.TotalUnits);
        }
    }
    public static void RepeatedTransfersDoNotDuplicateStatsOrItems()
    {
        using (var r = new InventoryTransferRig())
        {
            var old = r.EquipFirst(); var next = r.Gear(4); r.Inventory.SetItemAt(1, next);
            for (int i = 0; i < 6; i++)
            {
                r.Equipment.EquipItem(next); r.Equipment.EquipItem(next);
                Assert.AreEqual(130f, r.Attack); Assert.AreEqual(2, r.TotalUnits);
                r.Equipment.EquipItem(old);
                Assert.AreEqual(115f, r.Attack); Assert.AreEqual(2, r.TotalUnits);
            }
            r.Equipment.UnequipItem(ItemEnums.EquipItemType.Weapon, 0);
            r.Equipment.UnequipItem(ItemEnums.EquipItemType.Weapon);
            Assert.AreSame(old, r.Inventory.Items[0]); Assert.AreEqual(100f, r.Attack);
            Assert.AreEqual(2, r.TotalUnits);
        }
    }
    public static void InvalidUnequipRequestsAreNoOps()
    {
        using (var r = new InventoryTransferRig())
        {
            var gear = r.EquipFirst();
            foreach (int index in new[] { -1, 2, int.MaxValue })
                Assert.DoesNotThrow(() => r.Equipment.UnequipItem(ItemEnums.EquipItemType.Weapon, index));
            Assert.DoesNotThrow(() => r.Equipment.UnequipItem((ItemEnums.EquipItemType)100));
            Assert.AreSame(gear, r.Equipment.equipItems[ItemEnums.EquipItemType.Weapon]);
            Assert.AreEqual(115f, r.Attack); Assert.AreEqual(1, r.TotalUnits);
        }
    }
    public static void UnownedAndInvalidGearCannotBeEquipped()
    {
        using (var r = new InventoryTransferRig())
        {
            var external = r.Gear(); r.Equipment.EquipItem(external);
            r.Equipment.EquipItem(null);
            var invalid = r.Gear(2, (ItemEnums.EquipItemType)100); r.Inventory.SetItemAt(0, invalid);
            r.Equipment.EquipItem(invalid);
            var badData = r.Gear(); badData.itemBaseSO = null; r.Equipment.EquipItem(badData);
            Assert.IsNull(r.Equipment.equipItems[ItemEnums.EquipItemType.Weapon]);
            Assert.AreSame(invalid, r.Inventory.Items[0]); Assert.AreEqual(100f, r.Attack);
            Assert.AreEqual(1, external.currentStack);
        }
    }
    public static void SetItemAtCannotOverwriteOrDuplicate()
    {
        using (var r = new InventoryTransferRig())
        {
            var a = r.Gear(); var b = r.Gear(4); r.Inventory.SetItemAt(0, a);
            r.Inventory.SetItemAt(0, b); r.Inventory.SetItemAt(1, a); r.Inventory.SetItemAt(0, null);
            Assert.AreSame(a, r.Inventory.Items[0]); Assert.IsNull(r.Inventory.Items[1]);
            r.Equipment.EquipItem(a); r.Inventory.SetItemAt(1, a); r.Inventory.AddItem(a);
            Assert.IsNull(r.Inventory.Items[1]); Assert.AreEqual(1, r.TotalUnits);
        }
    }
    public static void InventorySwapPreservesItemsAndNotifies()
    {
        using (var r = new InventoryTransferRig(3))
        {
            var a = r.Gear(); var b = new NormalItemBase(r.Material, 3);
            r.Inventory.SetItemAt(0, a); r.Inventory.SetItemAt(1, b);
            int updates = 0; r.Inventory.OnInventoryUpdated += () => updates++;
            r.Inventory.Swap(0, 1); r.Inventory.Swap(0, 2);
            Assert.AreSame(a, r.Inventory.Items[1]); Assert.AreSame(b, r.Inventory.Items[2]);
            Assert.IsNull(r.Inventory.Items[0]); Assert.AreEqual(4, r.TotalUnits); Assert.AreEqual(2, updates);
            foreach (int index in new[] { -1, 3, int.MaxValue }) r.Inventory.Swap(0, index);
            r.Inventory.Swap(1, 1); Assert.AreEqual(2, updates);
        }
    }
    public static void TransferEventsObserveCommittedStateAndDoNotAcquireAgain()
    {
        using (var r = new InventoryTransferRig())
        {
            var old = r.EquipFirst(); var next = r.Gear(4); r.Inventory.SetItemAt(1, next);
            int acquired = 0, equipped = 0, unequipped = 0, updates = 0;
            GameEvent.OnGetItem = _ => acquired++;
            r.Inventory.OnInventoryUpdated += () =>
            {
                updates++;
                Assert.AreSame(next, r.Equipment.equipItems[ItemEnums.EquipItemType.Weapon]);
                Assert.AreSame(old, r.Inventory.Items[1]); Assert.AreEqual(130f, r.Attack);
                // Reentrant transfers during notification must not partially undo the current transaction.
                r.Equipment.UnequipItem(ItemEnums.EquipItemType.Weapon);
            };
            r.Equipment.OnEquipItem += _ => equipped++;
            r.Equipment.OnUnequipItem += _ => unequipped++;
            r.Equipment.EquipItem(next);
            Assert.AreEqual(0, acquired); Assert.AreEqual(1, updates);
            Assert.AreEqual(1, equipped); Assert.AreEqual(1, unequipped);
        }
    }
    public static void PartialAcquisitionKeepsDonorRemainder()
    {
        using (var r = new InventoryTransferRig(1))
        {
            var stored = new NormalItemBase(r.Material, 8); r.Inventory.SetItemAt(0, stored);
            var donor = new NormalItemBase(r.Material, 5);
            int updates = 0; r.Inventory.OnInventoryUpdated += () => updates++;
            r.Inventory.AddItem(donor);
            Assert.AreEqual(10, stored.currentStack); Assert.AreEqual(3, donor.currentStack);
            Assert.AreEqual(13, r.TotalUnits + donor.currentStack); Assert.AreEqual(1, updates);
            r.Inventory.AddItem(stored); r.Inventory.AddItem(donor);
            Assert.AreEqual(10, stored.currentStack); Assert.AreEqual(3, donor.currentStack);
            Assert.AreEqual(1, updates);
        }
    }
    public static void EquipmentCapacityIsOnePerEmptySlot()
    {
        using (var r = new InventoryTransferRig(1))
        {
            var gear = r.Gear(); gear.itemBaseSO.maxStack = 99; gear.itemBaseSO.stackable = true;
            Assert.IsTrue(r.Inventory.HasEnoughSpace(gear.itemBaseSO, 1));
            Assert.IsFalse(r.Inventory.HasEnoughSpace(gear.itemBaseSO, 2));
            r.Inventory.SetItemAt(0, gear);
            Assert.IsFalse(r.Inventory.HasEnoughSpace(gear.itemBaseSO, 1));
            Assert.IsFalse(r.Inventory.HasEnoughSpace(null, 1));
        }
    }
}

public sealed class InventorySlotTestRig : IDisposable
{
    public readonly GameObject Root = new GameObject("TransferTestCanvas", typeof(Canvas));
    public readonly InventorySlot[] Slots;
    public readonly EquipmentSlot EquippedSlot;
    public readonly EventSystem Events;
    private readonly GameObject eventObject = new GameObject("TransferTestEvents");
    public InventorySlotTestRig(InventoryTransferRig data)
    {
        eventObject.SetActive(false);
        Events = eventObject.AddComponent<EventSystem>();
        Slots = new InventorySlot[data.Inventory.Items.Count];
        for (int i = 0; i < Slots.Length; i++)
        {
            var go = new GameObject("Slot" + i, typeof(RectTransform)); go.transform.SetParent(Root.transform);
            var slot = go.AddComponent<InventorySlot>();
            InventoryTransferRig.Set(slot, "iconImage", go.AddComponent<Image>());
            var text = new GameObject("Stack", typeof(RectTransform), typeof(TextMeshProUGUI));
            text.transform.SetParent(go.transform);
            InventoryTransferRig.Set(slot, "stackText", text.GetComponent<TMP_Text>());
            slot.Init(data.Inventory, data.Equipment, i, null); Slots[i] = slot;
        }
        var equipment = new GameObject("Equipped", typeof(RectTransform)); equipment.transform.SetParent(Root.transform);
        EquippedSlot = equipment.AddComponent<EquipmentSlot>();
        InventoryTransferRig.Set(EquippedSlot, "iconImage", equipment.AddComponent<Image>());
        EquippedSlot.Init(data.Equipment, ItemEnums.EquipItemType.Weapon, null);
    }
    public PointerEventData Drag(GameObject source) => new PointerEventData(Events) { pointerDrag = source };
    public void Dispose() { UnityEngine.Object.DestroyImmediate(Root); UnityEngine.Object.DestroyImmediate(eventObject); }
}

public static class InventoryTransferRuntimeCases
{
    public static IEnumerator GearDropSwapsAndUpdatesIcons()
    {
        using (var r = new InventoryTransferRig())
        {
            var old = r.EquipFirst(); var next = r.Gear(4); r.Inventory.SetItemAt(0, next);
            using (var ui = new InventorySlotTestRig(r))
            {
                var e = ui.Drag(ui.Slots[0].gameObject);
                ui.Slots[0].OnBeginDrag(e); ui.EquippedSlot.OnDrop(e); ui.Slots[0].OnEndDrag(e);
                Assert.AreSame(next, ui.EquippedSlot.EquipItem); Assert.AreSame(old, ui.Slots[0].CurrentItem);
                Assert.IsTrue(ui.Slots[0].GetComponent<Image>().enabled);
                Assert.IsTrue(ui.EquippedSlot.GetComponent<Image>().enabled);
                Assert.AreEqual(130f, r.Attack); Assert.AreEqual(2, r.TotalUnits);
                e = ui.Drag(ui.EquippedSlot.gameObject);
                ui.EquippedSlot.OnBeginDrag(e); ui.Slots[0].OnDrop(e); ui.EquippedSlot.OnEndDrag(e);
                Assert.AreSame(old, ui.EquippedSlot.EquipItem); Assert.AreSame(next, ui.Slots[0].CurrentItem);
                yield return null;
                Assert.IsNull(ui.Root.transform.Find("DragIcon"));
            }
        }
    }
    public static IEnumerator RejectedEquipmentDropRestoresDragIcon()
    {
        using (var r = new InventoryTransferRig())
        {
            var old = r.EquipFirst(); var material = new NormalItemBase(r.Material, 6);
            r.Inventory.SetItemAt(0, material);
            using (var ui = new InventorySlotTestRig(r))
            {
                var e = ui.Drag(ui.EquippedSlot.gameObject);
                ui.EquippedSlot.OnBeginDrag(e); ui.Slots[0].OnDrop(e); ui.EquippedSlot.OnEndDrag(e);
                Assert.AreSame(old, ui.EquippedSlot.EquipItem); Assert.AreSame(material, ui.Slots[0].CurrentItem);
                Assert.IsTrue(ui.EquippedSlot.GetComponent<Image>().enabled); Assert.AreEqual(115f, r.Attack);
                ui.Slots[0].OnDrop(null); ui.EquippedSlot.OnDrop(new PointerEventData(ui.Events));
                yield return null; Assert.IsNull(ui.Root.transform.Find("DragIcon"));
            }
        }
    }
    public static IEnumerator StaleInventoryDragCannotEquipReplacement()
    {
        using (var r = new InventoryTransferRig())
        {
            var a = r.Gear(); var b = r.Gear(4); r.Inventory.SetItemAt(0, a);
            using (var ui = new InventorySlotTestRig(r))
            {
                var e = ui.Drag(ui.Slots[0].gameObject); ui.Slots[0].OnBeginDrag(e);
                r.Inventory.RemoveItem(a); r.Inventory.SetItemAt(0, b);
                ui.EquippedSlot.OnDrop(e); ui.Slots[1].OnDrop(e); ui.Slots[0].OnEndDrag(e);
                Assert.AreSame(b, r.Inventory.Items[0]); Assert.IsNull(ui.EquippedSlot.EquipItem);
                Assert.AreEqual(100f, r.Attack);
                yield return null; Assert.IsNull(ui.Root.transform.Find("DragIcon"));
            }
        }
    }
    public static IEnumerator ForeignInventoryDragIsRejected()
    {
        using (var a = new InventoryTransferRig())
        using (var b = new InventoryTransferRig())
        {
            var item = a.Gear(); a.Inventory.SetItemAt(0, item);
            using (var from = new InventorySlotTestRig(a))
            using (var to = new InventorySlotTestRig(b))
            {
                var e = from.Drag(from.Slots[0].gameObject); from.Slots[0].OnBeginDrag(e);
                to.Slots[0].OnDrop(e); to.EquippedSlot.OnDrop(e); from.Slots[0].OnEndDrag(e);
                Assert.AreSame(item, a.Inventory.Items[0]); Assert.IsNull(b.Inventory.Items[0]);
                Assert.IsNull(b.Equipment.equipItems[ItemEnums.EquipItemType.Weapon]);
                yield return null;
            }
        }
    }
    public static IEnumerator ClosingUIInvalidatesDrag()
    {
        using (var r = new InventoryTransferRig())
        {
            var item = r.Gear(); r.Inventory.SetItemAt(0, item);
            using (var ui = new InventorySlotTestRig(r))
            {
                var e = ui.Drag(ui.Slots[0].gameObject); ui.Slots[0].OnBeginDrag(e);
                ui.Root.SetActive(false); ui.Root.SetActive(true); ui.EquippedSlot.OnDrop(e);
                Assert.AreSame(item, r.Inventory.Items[0]); Assert.IsNull(ui.EquippedSlot.EquipItem);
                Assert.IsTrue(ui.Slots[0].GetComponent<Image>().enabled, "Reopening the UI must restore the item's icon.");
                yield return null; Assert.IsNull(ui.Root.transform.Find("DragIcon"));
            }
        }
    }
    public static IEnumerator GroundDropKeepsRemainderAndReturnsOnlyWhenCollected()
    {
        using (var r = new InventoryTransferRig(1))
        {
            var previousPool = PoolManager.Instance;
            typeof(PoolManager).GetProperty("Instance").SetValue(null, null);
            var poolObject = new GameObject("DropTransferPool"); var pool = poolObject.AddComponent<PoolManager>();
            var prefab = new GameObject("DropTransferPrefab"); prefab.SetActive(false);
            var template = prefab.AddComponent<DropItemModel>(); template.enabled = false;
            try
            {
                pool.CreatePool(template, 1, false);
                var drop = (DropItemModel)pool.Pop(prefab.name);
                drop.InitItem(r.Material, 5); drop.SetLock(false); drop.EnableInteract(r.Model.transform);
                var guide = new GameObject("Guide"); guide.transform.SetParent(drop.transform); drop.interactUI = guide;
                var stored = new NormalItemBase(r.Material, 8); r.Inventory.SetItemAt(0, stored);
                Assert.IsTrue(drop.Interact(r.Model.transform));
                Assert.AreEqual(10, stored.currentStack); Assert.IsTrue(drop.gameObject.activeSelf);
                Assert.IsTrue(guide.activeSelf);
                var remainder = (ItemBase)typeof(DropItemModel).GetField("dropItem", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(drop);
                Assert.AreEqual(3, remainder.currentStack);
                Assert.IsFalse(drop.Interact(r.Model.transform)); Assert.AreEqual(3, remainder.currentStack);
                Assert.IsTrue(guide.activeSelf);
                Assert.IsFalse(drop.Interact(null));
                r.Inventory.RemoveItem(stored);
                Assert.IsTrue(drop.Interact(r.Model.transform));
                Assert.AreEqual(3, r.Inventory.GetTotalItemCount(r.Material.itemID));
                Assert.IsFalse(drop.gameObject.activeSelf); Assert.AreEqual(0, remainder.currentStack);
                Assert.IsFalse(drop.Interact(r.Model.transform));
                var pools = (IDictionary)typeof(PoolManager).GetField("_pools", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pool);
                var stack = (ICollection)pools[prefab.name].GetType().GetField("_pool", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pools[prefab.name]);
                Assert.AreEqual(1, stack.Count);
                var reused = (DropItemModel)pool.Pop(prefab.name); reused.InitItem(null, 1);
                reused.SetLock(false); reused.EnableInteract(r.Model.transform);
                Assert.IsFalse(reused.Interact(r.Model.transform));
                pool.Push(reused);
                yield return null;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(poolObject); UnityEngine.Object.DestroyImmediate(prefab);
                typeof(PoolManager).GetProperty("Instance").SetValue(null, previousPool);
            }
        }
    }
}
