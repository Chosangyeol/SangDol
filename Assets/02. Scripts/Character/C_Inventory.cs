using System;
using System.Collections.Generic;
using UnityEngine;

public class C_Inventory
{
    private CharacterModel owner;
    public CharacterModel Owner => owner;

    private List<ItemBase> items;
    public List<ItemBase> Items => items;

    public int slotSize = 30;


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

    public C_Inventory(CharacterModel model, int slotSize)
    {
        owner = model;
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
}
