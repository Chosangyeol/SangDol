using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NpcShopManager : MonoBehaviour
{
    public static NpcShopManager instance;

    [Header("UI 연결")]
    public GameObject shopPanel;
    public Transform gridLayoutGroup;
    public GameObject shopSlotPrefab;

    [Header("아이템 데이터베이스")]
    public ItemDataBaseSO globalItemData;

    private CharacterModel _model;
    public CharacterModel model => _model;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        _model = FindAnyObjectByType<CharacterModel>();
    }

    public void OpenShop(string shopID)
    {
        shopPanel.SetActive(true);

        if (!UIManager.Instance.inventoryUI.gameObject.activeSelf)
            UIManager.Instance.inventoryUI.Toggle();

        LoadShopDataFromCSV(shopID);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    private void LoadShopDataFromCSV(string shopID)
    {
        foreach (Transform child in gridLayoutGroup)
        {
            Destroy(child.gameObject);
        }

        TextAsset csvData = Resources.Load<TextAsset>($"ShopData/{shopID}");
        if (csvData == null) return;

        string[] lines = csvData.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] row = lines[i].Split(',');
            
            ItemBaseSO itemSO = globalItemData.GetItemByID(row[0]);
            int.TryParse(row[1], out int itemPrice);

            if (itemSO != null)
            {
                GameObject slotObj = Instantiate(shopSlotPrefab, gridLayoutGroup);
                ShopSlotUI slotUI = slotObj.GetComponent<ShopSlotUI>();
                slotUI.InitSlot(itemSO, itemPrice);
            }
        }
    }

    public void BuyItem(ItemBaseSO itemSO,int price, int amount)
    {
        int totalCost = price * amount;

        if (_model.Stat.Stat.gold < totalCost)
        {
            Debug.Log("골드 부족");
            return;
        }

        if (!model.Inventory.HasEnoughSpace(amount, itemSO.maxStack))
        {
            Debug.Log("공간 부족");
            return;
        }

        model.UseGold(totalCost);

        int remainingAmount = amount;

        while (remainingAmount > 0)
        {
            int stackToAdd = Mathf.Min(remainingAmount, itemSO.maxStack);
            ItemBase newItem = itemSO.CreateItem(stackToAdd);

            model.Inventory.AddItem(newItem);

            remainingAmount -= stackToAdd;
        }
        
    }
}
