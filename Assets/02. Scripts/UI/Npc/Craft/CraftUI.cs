using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftUI : MonoBehaviour
{
    [Header("제작 레시피 목록 UI")]
    public GameObject gridLayout;
    public List<GameObject> slots = new List<GameObject>();
    public GameObject slotUI;
    public ItemTooltip tooltip;

    [Header("제작 실행 UI")]
    public Image resultItemIcon;
    public TMP_Text resultItemNameText;
    public TMP_Text resultItemRarityText;
    public TMP_Text requireGoldAmountText;
    public Image[] materialImages;
    public TMP_Text[] materialCountTexts;
    public GameObject executeButton;

    public void SetCraftRecipeUI(List<CraftRecipe> recipeList)
    {
        for (int n = 0; n < slots.Count; n++)
            Destroy(slots[n]);

        slots.Clear();

        for (int i = 0; i < recipeList.Count; i++)
        {
            GameObject slot = Instantiate(slotUI, gridLayout.transform);
            slot.GetComponent<CraftRecipeSlot>().Init(recipeList[i], tooltip);
            slots.Add(slot);
        }
    }

    public void SetCraftExecuteUI(CraftRecipe recipe, CharacterModel model)
    {
        if (recipe == null)
        {
            resultItemIcon.sprite = null;
            resultItemNameText.text = "";
            resultItemRarityText.text = "";

            requireGoldAmountText.text = "";

            for(int i = 0; i < 4; i++)
            {
                materialCountTexts[i].text = "";
                materialImages[i].sprite = null;
            }
            executeButton.SetActive(false);

            return;
        }

        C_Inventory inventory = model.Inventory;

        bool canCraft = true;

        ItemBaseSO result = ItemManager.Instance.GetItemBaseSO(recipe.resultItemId);
        string rarity = ItemManager.Instance.ReturnRarity(result.itemRarity);

        resultItemIcon.sprite = result.itemIcon;
        resultItemNameText.text = result.itemName;
        resultItemRarityText.text = $"등급 : {rarity}";

        requireGoldAmountText.text = $"{model.Stat.Stat.gold} / {recipe.requireGoldAmount}";

        if (model.Stat.Stat.gold < recipe.requireGoldAmount)
        {
            canCraft = false;
            requireGoldAmountText.color = Color.red;
        }
        else
            requireGoldAmountText.color = Color.yellow;

        int index = 0;

        for (int i = 0; i < recipe.materialItemIds.Length; i++)
        {
            string targetID = recipe.materialItemIds[i];
            ItemBaseSO target = ItemManager.Instance.GetItemBaseSO(targetID);

            materialImages[i].sprite = target.itemIcon;
            materialCountTexts[i].text =
                $"{inventory.GetTotalItemCount(targetID)} / {recipe.materialItemAmounts[i]}";
        
            if (inventory.GetTotalItemCount(targetID) < recipe.materialItemAmounts[i])
            {
                canCraft = false;
                materialCountTexts[i].color = Color.red;
            }
            else
                materialCountTexts[i].color = Color.yellow;

            index++;
        }

        for (int i = index; i < recipe.materialItemIds.Length;i++)
        {
            materialCountTexts[i].text = "";
            materialImages[i].sprite = null;
        }
        executeButton.SetActive(canCraft);
    }
}
