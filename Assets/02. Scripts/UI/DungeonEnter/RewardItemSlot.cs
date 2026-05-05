using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RewardItemSlot : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private ItemBaseSO slotItem;

    [Header("UI 연결")]
    public Image itemIcon;

    private ItemTooltip tooltip;

    public void InitSlot(ItemBaseSO slotItem, ItemTooltip tooltip)
    {
        this.slotItem = slotItem;
        itemIcon.sprite = this.slotItem.itemIcon;

        this.tooltip = tooltip;
    }

    public void ClearSlot()
    {
        this.slotItem = null;
        itemIcon.sprite = null;
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

        tooltip.ToggleTooltip(false, null, (ItemBaseSO)null);
    }
}
