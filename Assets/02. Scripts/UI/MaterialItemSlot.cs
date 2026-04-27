using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MaterialItemSlot : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("재료 아이템 정보")]
    public ItemBaseSO materialItem;

    [Header("UI 정보")]
    [SerializeField] private Image iconImage;

    private ItemTooltip tooltip;

    public void Init(ItemTooltip tooltip)
    {
        this.tooltip = tooltip;
    }

    public void SetUpItem(ItemBaseSO item = null)
    {
        if (item == null)
        {
            materialItem = null;
            iconImage.sprite = null;
        }

        materialItem = item;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (materialItem == null) return;
        if (tooltip == null) return;

        tooltip.ToggleTooltip(true, this.GetComponent<RectTransform>(), materialItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (materialItem == null) return;
        if (tooltip == null) return;

        tooltip.ToggleTooltip(false, null, (ItemBaseSO)null);
    }

}
