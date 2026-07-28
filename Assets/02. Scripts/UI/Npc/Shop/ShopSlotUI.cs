using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private ItemBaseSO slotItem;
    private int itemPrice;

    [Header("UI 연결")]
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text itemPrictText;

    private ItemTooltip tooltip;

    public void InitSlot(ItemBaseSO item, int price, ItemTooltip tooltip)
    {
        slotItem = item;
        itemPrice = price;
        this.tooltip = tooltip;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slotItem == null) return;
        if (tooltip == null) return;

        tooltip.ToggleTooltip(true, this.GetComponent<RectTransform>(), slotItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (slotItem == null) return;
        if (tooltip == null) return;

        tooltip.ToggleTooltip(false,null,(ItemBaseSO)null);
    }
}
