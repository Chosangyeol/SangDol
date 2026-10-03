using System;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class C_Inventory
{
    private CharacterModel _model;
    public CharacterModel Model => _model;

    private List<ItemBase> items;
    public List<ItemBase> Items => items;

    public int slotSize = 30;

    public Dictionary<C_Enums.UseSlot, int> useSlots =
        new Dictionary<C_Enums.UseSlot, int>()
        {
            { C_Enums.UseSlot.Slot_1, 99 },
            { C_Enums.UseSlot.Slot_2, 99 },
            { C_Enums.UseSlot.Slot_3, 99 },
            { C_Enums.UseSlot.Slot_4, 99 }
        };

    /// <summary>
    /// 인벤토리에 아이템이 추가된 후 호출되는 이벤트
    /// EX) UI 업데이트
    /// </summary>
    public event Action<ItemBase> OnAddItemInventory;

    /// <summary>
    /// 인벤토리에 아이템이 제거된 후 호출되는 이벤트
    /// EX) UI 업데이트
    /// </summary>
    public event Action<ItemBase> OnRemoveItemInventory;

    public event Action OnInventoryUpdated;

    public C_Inventory(CharacterModel model, int slotSize)
    {
        _model = model;
        this.slotSize = slotSize;
        items = new List<ItemBase>(slotSize);
        for (int i = 0; i < slotSize; i++)
        {
            items.Add(null);
        }

        return;
    }

    /// <summary>
    /// 인벤토리에 아이템을 추가하는 함수
    /// </summary>
    /// <param name="item">인벤토리에 추가할 아이템</param>
    public void AddItem(ItemBase item) => AddItemWithResult(item);

    internal int AddItemWithResult(ItemBase item)
    {
        if (item == null || item.itemBaseSO == null || item.currentStack <= 0 || item.maxStack <= 0 ||
            items.Contains(item) || (_model != null && _model.Equipment != null &&
            item is EquipItemBase equipped && _model.Equipment.equipItems.ContainsValue(equipped))) return 0;
        int before = item.currentStack;
        var added = new List<ItemBase>();
        AddItemCore(item, added);
        int received = before - item.currentStack;
        if (received > 0)
        {
            foreach (var entry in added) { entry.OnAddInventory(); OnAddItemInventory?.Invoke(entry); }
            // A full stack merge has no new-slot hook, but is still an acquisition.
            if (added.Count == 0) GameEvent.OnGetItem?.Invoke(item.itemBaseSO.itemID);
            OnInventoryUpdated?.Invoke();
        }
        return received;
    }

    private void AddItemCore(ItemBase item, List<ItemBase> added)
    {

        if (item == null) return;
        if (item.currentStack < 1) return;

        // 1. 스택 가능한 아이템이면 먼저 기존 스택에 채움
        if (item.itemBaseSO.stackable && !(item is EquipItemBase))
        {
            for (int i = 0; i < items.Count; i++)
            {
                ItemBase nowItem = items[i];
                if (nowItem == null) continue;
                if (nowItem.itemBaseSO.itemID != item.itemBaseSO.itemID) continue;
                if (nowItem.currentStack >= nowItem.maxStack) continue;

                int leftStack = nowItem.maxStack - nowItem.currentStack;
                int addStack = Mathf.Min(leftStack, item.currentStack);

                nowItem.currentStack += addStack;
                item.currentStack -= addStack;

                if (item.currentStack <= 0)
                    return;
            }
        }

        // 2. 남은 수량을 빈칸에 추가
        while (item.currentStack > 0)
        {
            int emptyIndex = FindEmptySlot();
            if (emptyIndex == -1)
            {
                Debug.Log("인벤토리 공간 부족");
                return;
            }

            int addStack = item.itemBaseSO.stackable && !(item is EquipItemBase)
                ? Mathf.Min(item.maxStack, item.currentStack)
                : 1;

            ItemBase newItem = item.Clone(addStack);
            items[emptyIndex] = newItem;

            added.Add(newItem);

            item.currentStack -= addStack;
        }
    }

    // All placements are planned without mutating existing references or quantities.
    // commitBeforeNotifications must commit owner state; failures in external subscribers
    // after commit are not a retryable inventory-capacity failure.
    internal bool TryAddRewards(IReadOnlyList<KeyValuePair<ItemBaseSO, int>> rewards, Action commitBeforeNotifications)
    {
        if (rewards == null || commitBeforeNotifications == null) return false;
        var planned = new List<ItemBase>(items);
        var counts = new long[items.Count];
        for (int i = 0; i < items.Count; i++) counts[i] = items[i]?.currentStack ?? 0;
        var added = new List<ItemBase>();
        var mergedIDs = new HashSet<string>();
        var newIDs = new HashSet<string>();
        foreach (var reward in rewards)
        {
            var data = reward.Key;
            if (data == null || string.IsNullOrWhiteSpace(data.itemID) || reward.Value <= 0) return false;
            bool stackable = data.stackable && !(data is EquipItemSO);
            int perSlot = stackable ? data.maxStack : 1;
            if (perSlot <= 0) return false;
            long remaining = reward.Value;
            if (stackable)
            {
                for (int i = 0; i < planned.Count && remaining > 0; i++)
                {
                    var entry = planned[i];
                    if (entry == null || entry is EquipItemBase || entry.itemBaseSO == null ||
                        entry.itemBaseSO.itemID != data.itemID) continue;
                    long amount = Math.Min(remaining, Math.Max(0L, (long)entry.maxStack - counts[i]));
                    counts[i] += amount; remaining -= amount;
                    if (amount > 0) mergedIDs.Add(data.itemID);
                }
            }
            for (int i = 0; i < planned.Count && remaining > 0; i++)
            {
                if (planned[i] != null) continue;
                int amount = (int)Math.Min(remaining, perSlot);
                var entry = data.CreateItem(amount);
                if (entry == null || entry.itemBaseSO == null || entry.currentStack != amount || entry.maxStack < amount) return false;
                planned[i] = entry; counts[i] = amount; remaining -= amount;
                added.Add(entry); newIDs.Add(data.itemID);
            }
            if (remaining > 0) return false;
        }
        for (int i = 0; i < items.Count; i++)
        {
            items[i] = planned[i];
            if (items[i] != null) items[i].currentStack = (int)counts[i];
        }
        commitBeforeNotifications();
        foreach (var entry in added) { entry.OnAddInventory(); OnAddItemInventory?.Invoke(entry); }
        foreach (var id in mergedIDs) if (!newIDs.Contains(id)) GameEvent.OnGetItem?.Invoke(id);
        if (rewards.Count > 0) OnInventoryUpdated?.Invoke();
        return true;
    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
                return i;
        }
        return -1;
    }

    public int GetEmptySlotCount()
    {
        int emptyCount = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                emptyCount++; // 빈 칸을 발견할 때마다 개수 증가
            }
        }
        return emptyCount;
    }

    public bool HasEnoughSpace(ItemBaseSO item, int buyAmount)
    {
        if (item == null || buyAmount <= 0) return item != null;
        bool stackable = item.stackable && !(item is EquipItemSO);
        int perSlot = stackable ? item.maxStack : 1;
        if (perSlot <= 0) return false;
        long availableCapacity = 0;
        foreach (ItemBase inventoryItem in items)
        {
            if (inventoryItem == null) availableCapacity += perSlot;
            else if (stackable && inventoryItem.itemBaseSO != null && inventoryItem.itemBaseSO.itemID == item.itemID)
                availableCapacity += Mathf.Max(0, inventoryItem.maxStack - inventoryItem.currentStack);
            if (availableCapacity >= buyAmount) return true;
        }
        return false;
    }

    /// <summary>
    /// 인벤토리에 있는 아이템의 지속효과를 작동시키는 함수
    /// 대부분의 상황에서 delta는 Time.deltaTime이 들어감
    /// </summary>
    /// <param name="delta"></param>
    public void UpdateItem(float delta)
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i]?.OnUpdateInventory(delta);
        }
    }

    /// <summary>
    /// 인벤토리에 아이템을 제거하는 함수
    /// </summary>
    /// <param name="item">인벤토리에서 제거할 아이템</param>
    public void RemoveItem(ItemBase item)
    {
        if (item == null) return;
        int index = items.IndexOf(item);
        if (index < 0) return;
        items[index] = null;
        item.OnRemoveInventory();
        OnRemoveItemInventory?.Invoke(item);
        OnInventoryUpdated?.Invoke();
    }

    public void RemoveItemAt(int index)
    {
        if (index < 0 || index >= items.Count || items[index] == null) return;
        ItemBase item = items[index]; items[index] = null;
        item.OnRemoveInventory();
        OnRemoveItemInventory?.Invoke(item);
        OnInventoryUpdated?.Invoke();
    }

    public void SetItemAt(int index, ItemBase item)
    {
        if (item is EquipItemBase equipped && _model != null && _model.Equipment != null &&
            _model.Equipment.equipItems.ContainsValue(equipped)) return;
        if (item != null && TryReplaceTransferredItem(index, null, item)) NotifyTransfer(null, item);
    }

    internal bool TryReplaceTransferredItem(int index, ItemBase expected, ItemBase replacement)
    {
        if (index < 0 || index >= items.Count || !ReferenceEquals(items[index], expected)) return false;
        if (ReferenceEquals(expected, replacement)) return false;
        if (replacement != null && (replacement.itemBaseSO == null || replacement.currentStack <= 0 ||
            replacement.maxStack <= 0 || replacement.currentStack > replacement.maxStack || items.Contains(replacement))) return false;
        items[index] = replacement;
        return true;
    }

    internal void NotifyTransfer(ItemBase removed, ItemBase added)
    {
        if (removed != null) { removed.OnRemoveInventory(); OnRemoveItemInventory?.Invoke(removed); }
        // Transfers are not new acquisitions; do not repeat OnAddInventory / OnGetItem.
        if (added != null) OnAddItemInventory?.Invoke(added);
        OnInventoryUpdated?.Invoke();
    }

    public void Swap(int from, int to) => TrySwap(from, to);

    internal bool TrySwap(int from, int to)
    {
        if (from < 0 || to < 0 || from >= items.Count || to >= items.Count || from == to) return false;
        if (items[from] == null && items[to] == null) return false;
        (items[from], items[to]) = (items[to], items[from]);
        OnInventoryUpdated?.Invoke();
        return true;
    }

    public void UseItem(C_Enums.UseSlot slot)
    {
        if (!_model.canUse) return;

        if (slot == C_Enums.UseSlot.None) return;

        int index = useSlots[slot];

        if (index == 99) return;

        UseItemBase useItem = Items[index] as UseItemBase;

        if (useItem == null) return;

        if (useItem.UseItem(_model))
        {
            Items[index].currentStack--;
            Debug.Log(Items[index].currentStack);
            if (Items[index].currentStack <= 0)
                RemoveItemAt(index);
        }

        OnInventoryUpdated?.Invoke();
    }

    public int GetTotalItemCount(string targetItemID)
    {
        int totalCount = 0;

        foreach (var item in Items)
        {
            if (item != null && item.itemBaseSO.itemID == targetItemID)
            {
                totalCount += item.currentStack;
            }
        }

        return totalCount;
    }

    public void RemoveTargetItem(string targetItemID, int amount)
    {
        if (string.IsNullOrEmpty(targetItemID) || amount <= 0 || GetTotalItemCount(targetItemID) < amount) return;
        int remaining = amount;
        var removed = new List<ItemBase>();
        for (int i = 0; i < items.Count && remaining > 0; i++)
        {
            var entry = items[i];
            if (entry == null || entry.itemBaseSO == null || entry.itemBaseSO.itemID != targetItemID) continue;
            int count = Mathf.Min(entry.currentStack, remaining);
            entry.currentStack -= count; remaining -= count;
            if (entry.currentStack <= 0) { items[i] = null; removed.Add(entry); }
        }
        foreach (var entry in removed) { entry.OnRemoveInventory(); OnRemoveItemInventory?.Invoke(entry); }
        OnInventoryUpdated?.Invoke();
    }
}
