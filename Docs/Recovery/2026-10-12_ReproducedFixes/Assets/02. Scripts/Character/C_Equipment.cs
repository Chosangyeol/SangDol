using System;
using System.Collections.Generic;
using UnityEngine;

public class C_Equipment
{
    public CharacterModel owner;

    /// <summary>
    /// 케릭터의 아이템 착용 슬룻을 저장하는 Dictionary
    /// 장착 아이템의 부위를 Key로 사용하고, 장착된 아이템을 Value로 사용
    /// </summary>
    public Dictionary<ItemEnums.EquipItemType, EquipItemBase> equipItems = new Dictionary<ItemEnums.EquipItemType, EquipItemBase>()
    {
        { ItemEnums.EquipItemType.Head, null },
        { ItemEnums.EquipItemType.Body, null },
        { ItemEnums.EquipItemType.Pants, null },
        { ItemEnums.EquipItemType.Gloves, null },
        { ItemEnums.EquipItemType.Shoes, null },
        { ItemEnums.EquipItemType.Weapon, null }
    };

    private bool changingEquipment;

    public event Action<EquipItemBase> OnEquipItem;
    public event Action<EquipItemBase> OnUnequipItem;

    public C_Equipment(CharacterModel owner)
    {
        this.owner = owner;
        return;
    }

    /// <summary>
    /// 아이템을 착용할 때 호출하는 메서드
    /// </summary>
    /// <param name="item">착용할 아이템</param>
    public void EquipItem(EquipItemBase item) => TryEquipItem(item);

    internal bool TryEquipItem(EquipItemBase item)
    {
        if (changingEquipment || owner == null || owner.Inventory == null || owner.Stat == null ||
            item == null || item.itemBaseSO == null || item.currentStack != 1 ||
            !equipItems.TryGetValue(item.itemBaseSO.equipItemType, out EquipItemBase previous)) return false;
        int index = owner.Inventory.Items.IndexOf(item);
        if (index < 0 || equipItems.ContainsValue(item)) return false;
        if (previous != null && (previous.itemBaseSO == null || previous.currentStack != 1)) return false;
        changingEquipment = true;
        try
        {
            if (!owner.Inventory.TryReplaceTransferredItem(index, item, previous)) return false;
            equipItems[item.itemBaseSO.equipItemType] = item;
            if (previous != null)
                owner.RemoveStat(previous.itemBaseSO.statToIncrease, previous.itemBaseSO.isPercent, previous.GetFinalStat());
            owner.AddStat(item.itemBaseSO.statToIncrease, item.itemBaseSO.isPercent, item.GetFinalStat());
            owner.Inventory.NotifyTransfer(item, previous);
            if (previous != null) OnUnequipItem?.Invoke(previous);
            OnEquipItem?.Invoke(item);
            if (UIManager.Instance != null) UIManager.Instance.RefreshAll();
            return true;
        }
        finally { changingEquipment = false; }
    }

    /// <summary>
    /// 아이템을 착용 해체할 때 호출하는 메서드
    /// </summary>
    /// <param name="equipItemType">착용 해체 할 아이템 부위</param>
    public void UnequipItem(ItemEnums.EquipItemType equipItemType, int inventoryIndex = 99)
        => TryUnequipItem(equipItemType, inventoryIndex);

    internal bool TryUnequipItem(ItemEnums.EquipItemType equipItemType, int inventoryIndex = 99)
    {
        if (changingEquipment || owner == null || owner.Inventory == null || owner.Stat == null ||
            !equipItems.TryGetValue(equipItemType, out EquipItemBase item) || item == null ||
            item.itemBaseSO == null || item.currentStack != 1) return false;
        int index = inventoryIndex == 99 ? owner.Inventory.FindEmptySlot() : inventoryIndex;
        if (index < 0 || index >= owner.Inventory.Items.Count) return false;
        ItemBase occupant = owner.Inventory.Items[index];
        if (occupant != null)
            return occupant is EquipItemBase replacement && replacement.itemBaseSO != null &&
                replacement.itemBaseSO.equipItemType == equipItemType && TryEquipItem(replacement);
        changingEquipment = true;
        try
        {
            if (!owner.Inventory.TryReplaceTransferredItem(index, null, item)) return false;
            equipItems[equipItemType] = null;
            owner.RemoveStat(item.itemBaseSO.statToIncrease, item.itemBaseSO.isPercent, item.GetFinalStat());
            owner.Inventory.NotifyTransfer(null, item);
            OnUnequipItem?.Invoke(item);
            if (UIManager.Instance != null) UIManager.Instance.RefreshAll();
            return true;
        }
        finally { changingEquipment = false; }
    }
}
