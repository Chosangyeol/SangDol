using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
    
{
    [SerializeField] InventorySlot slotPrefab;
    [SerializeField] Transform slotParent;
    [SerializeField] ItemTooltip tooltip;

    private RectTransform rectTransform;
    private Canvas canvas;


    private C_Inventory _inventory;
    private List<InventorySlot> slots = new();

    public void Init(C_Inventory inventory, C_Equipment equipment)
    {
        _inventory = inventory;
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        for (int i = 0; i < inventory.slotSize; i++)
        {
            InventorySlot slot = Instantiate(slotPrefab, slotParent);
            slot.Init(inventory,equipment, i, tooltip);
            slots.Add(slot);
        }

        BindInventoryEvents();
        RefreshAll();

        tooltip.gameObject.SetActive(false);
    }

    private void BindInventoryEvents()
    {
        _inventory.OnAddItemInventory += OnInvenytoryChange;
        _inventory.OnRemoveItemInventory += OnInvenytoryChange;
        _inventory.OnInventoryUpdated += RefreshAll;
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

    #region UI 이동
    

    #endregion
}
