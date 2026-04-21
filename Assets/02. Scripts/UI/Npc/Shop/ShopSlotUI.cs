using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour, IPointerClickHandler
{
    private ItemBaseSO slotItem;
    private int itemPrice;

    [Header("UI 연결")]
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text itemPrictText;

    public void InitSlot(ItemBaseSO item, int price)
    {
        slotItem = item;
        itemPrice = price;

        itemIcon.sprite = item.itemIcon;
        itemNameText.text = item.itemName;
        itemPrictText.text = itemPrice.ToString() + " G";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                ShopSetCountUI.instance.OpenUI(slotItem, itemPrice);
            }
            else
            {
                NpcShopManager.instance.BuyItem(slotItem, itemPrice, 1);
            }
        }
    }
}
