using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] InventorySlot slotPrefab;
    [SerializeField] Transform slotParent;

    private C_Inventory _inventory;
    private List<InventorySlot> slots = new();

    public void Init(C_Inventory inventory, C_Equipment equipment)
    {
        _inventory = inventory;

        for (int i = 0; i < inventory.slotSize; i++)
        {
            InventorySlot slot = Instantiate(slotPrefab, slotParent);
            slot.Init(inventory,equipment, i);
            slots.Add(slot);
        }

        BindInventoryEvents();
        RefreshAll();
    }

    private void BindInventoryEvents()
    {
        _inventory.OnAddItemInventory += OnInvenytoryChange;
        _inventory.OnRemoveItemInventory += OnInvenytoryChange;
    }

    private void OnInvenytoryChange(ItemBase _)
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        foreach (var slot in slots)
            slot.Refresh();
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
