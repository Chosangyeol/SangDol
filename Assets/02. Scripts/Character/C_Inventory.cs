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
    public void AddItem(ItemBase item)
    {

        if (item == null) return;
        if (item.currentStack < 1) return;

        // 1. 스택 가능한 아이템이면 먼저 기존 스택에 채움
        if (item.itemBaseSO.stackable)
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

            int addStack = item.itemBaseSO.stackable
                ? Mathf.Min(item.maxStack, item.currentStack)
                : 1;

            ItemBase newItem = item.Clone(addStack);
            items[emptyIndex] = newItem;

            newItem.OnAddInventory();
            OnAddItemInventory?.Invoke(newItem);

            item.currentStack -= addStack;
        }
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

    public bool HasEnoughSpace(int buyAmount, int itemMaxStack)
    {
        // 구매하려는 수량을 바탕으로 총 몇 칸의 슬롯이 필요한지 계산
        int requiredSlots = Mathf.CeilToInt((float)buyAmount / itemMaxStack);

        // GetEmptySlotCount()를 호출하여 현재 빈 칸이 몇 개인지 확인
        int emptySlots = GetEmptySlotCount();

        // 빈 칸이 필요한 칸 수보다 많거나 같으면 구매 가능(true)
        if (emptySlots >= requiredSlots)
        {
            return true;
        }
        else
        {
            Debug.Log($"<color=red>인벤토리 공간 부족! (필요한 빈 칸: {requiredSlots}, 현재 빈 칸: {emptySlots})</color>");
            return false;
        }
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
            items[i].OnUpdateInventory(delta);
        }
    }

    /// <summary>
    /// 인벤토리에 아이템을 제거하는 함수
    /// </summary>
    /// <param name="item">인벤토리에서 제거할 아이템</param>
    public void RemoveItem(ItemBase item)
    {
        int index = items.IndexOf(item);
        if (index < 0) return;

        items[index] = null;
        item.OnRemoveInventory();
        OnRemoveItemInventory?.Invoke(item);
    }

    public void RemoveItemAt(int index)
    {
        if (index < 0 || index >= items.Count) return;
        if (items[index] == null) return;

        ItemBase item = items[index];
        items[index] = null;

        item.OnRemoveInventory();
        OnRemoveItemInventory?.Invoke(item);
    }

    public void SetItemAt(int index, ItemBase item)
    {
        if (index < 0 || index >= items.Count) return;
        items[index] = item;
    }

    public void Swap(int from, int to)
    {
        if (from < 0 || to < 0) return;
        if (from >= items.Count || to >= items.Count) return;
        if (from == to) return;

        (items[from], items[to]) = (items[to], items[from]);
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
        int currentTotal = GetTotalItemCount(targetItemID);
        if (currentTotal < amount)
            return;

        int remainToRemove = amount;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].currentStack > remainToRemove)
            {
                items[i].currentStack -= remainToRemove;
                remainToRemove = 0;
                break;
            }
            else
            {
                remainToRemove -= items[i].currentStack;
                RemoveItem(items[i]);

                if (remainToRemove <= 0)
                    break;
            }
            
        }
    }
}
