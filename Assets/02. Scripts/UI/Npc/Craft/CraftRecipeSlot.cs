using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CraftRecipeSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("슬룻 UI")]
    public Image resultIcon;
    public TMP_Text resultName;
    public TMP_Text resultRarity;

    private ItemBaseSO nowItem;
    private CraftRecipe nowRecipe;

    public void Init(CraftRecipe recipe)
    {
        nowRecipe = recipe;

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
}
