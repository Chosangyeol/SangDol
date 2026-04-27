using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CraftRecipeSlot : MonoBehaviour, IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("슬룻 UI")]
    public Image resultIcon;
    public TMP_Text resultName;
    public TMP_Text resultRarity;

    private ItemBaseSO nowItem;
    private CraftRecipe nowRecipe;
    private ItemTooltip tooltip;

    public void Init(CraftRecipe recipe, ItemTooltip tooltip)
    {
        nowRecipe = recipe;
        this.tooltip = tooltip;

        nowItem = ItemManager.Instance.GetItemBaseSO(nowRecipe.resultItemId);

        resultIcon.sprite = nowItem.itemIcon;
        resultName.text = nowItem.itemName;
        resultRarity.text = $"등급 : {nowItem.itemRarity}";

    }

    public void OnPointerClick(PointerEventData eventData)
    { 
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            NpcCraftManager.instance.SelectCraftRecipe(nowRecipe);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (nowItem == null) return;
        if (nowRecipe == null) return;
        if (tooltip == null) return;

        tooltip.ToggleTooltip(true, this.GetComponent<RectTransform>(), nowItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (nowItem == null) return;
        if (nowRecipe == null) return;
        if (tooltip == null) return;

        tooltip.ToggleTooltip(false,null,(ItemBaseSO)null);
    }
}
