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

    private CraftRecipe previewRecipe;
    private CharacterModel previewModel;
    private C_Inventory previewInventory;
    private void OnEnable() { BindPreview(); RefreshPreview(); }
    private void OnDisable() => UnbindPreview();
    private void OnDestroy() => UnbindPreview();

    private void BindPreview()
    {
        UnbindPreview();
        if (!isActiveAndEnabled || previewRecipe == null || previewModel == null) return;
        previewInventory = previewModel.Inventory;
        if (previewInventory != null) previewInventory.OnInventoryUpdated += RefreshPreview;
        GameEvent.OnStatChange += RefreshStat;
    }
    private void UnbindPreview()
    {
        if (previewInventory != null) previewInventory.OnInventoryUpdated -= RefreshPreview;
        previewInventory = null;
        GameEvent.OnStatChange -= RefreshStat;
    }
    private void RefreshStat(CharacterStat stat)
    {
        if (previewModel != null && previewModel.Stat != null && previewModel.Stat.Stat == stat) RefreshPreview();
    }

    public void SetCraftRecipeUI(List<CraftRecipe> recipeList)
    {
        foreach (var slot in slots) Destroy(slot);
        slots.Clear();
        if (recipeList == null) return;
        foreach (var recipe in recipeList)
        {
            var slot = Instantiate(slotUI, gridLayout.transform);
            slot.GetComponent<CraftRecipeSlot>().Init(recipe, tooltip);
            slots.Add(slot);
        }
    }

    public void SetCraftExecuteUI(CraftRecipe recipe, CharacterModel model)
    {
        previewRecipe = recipe;
        previewModel = model;
        BindPreview();
        RefreshPreview();
    }

    private void ClearPreview()
    {
        if (resultItemIcon != null) resultItemIcon.sprite = null;
        if (resultItemNameText != null) resultItemNameText.text = "";
        if (resultItemRarityText != null) resultItemRarityText.text = "";
        if (requireGoldAmountText != null) requireGoldAmountText.text = "";
        if (materialImages != null)
            foreach (var image in materialImages) if (image != null) image.sprite = null;
        if (materialCountTexts != null)
            foreach (var text in materialCountTexts) if (text != null) text.text = "";
        if (executeButton != null) executeButton.SetActive(false);
    }

    private void RefreshPreview()
    {
        ClearPreview();
        var recipe = previewRecipe;
        var model = previewModel;
        if (recipe == null || model == null || model.Inventory == null || model.Stat == null) return;
        if (ItemManager.Instance == null || recipe.materialItemIds == null || recipe.materialItemAmounts == null ||
            recipe.materialItemIds.Length != recipe.materialItemAmounts.Length || materialImages == null ||
            materialCountTexts == null || recipe.materialItemIds.Length > materialImages.Length ||
            recipe.materialItemIds.Length > materialCountTexts.Length)
        {
            ShowFailure("제작 정보를 확인할 수 없습니다.");
            return;
        }
        var result = ItemManager.Instance.GetItemBaseSO(recipe.resultItemId);
        if (result == null) { ShowFailure("제작 결과 정보를 확인할 수 없습니다."); return; }

        if (resultItemIcon != null) resultItemIcon.sprite = result.itemIcon;
        if (resultItemNameText != null) resultItemNameText.text = result.itemName;
        if (resultItemRarityText != null) resultItemRarityText.text = "등급 : " + ItemManager.Instance.ReturnRarity(result.itemRarity);
        if (requireGoldAmountText != null)
        {
            requireGoldAmountText.text = model.Stat.Stat.gold + " / " + recipe.requireGoldAmount;
            requireGoldAmountText.color = Color.yellow;
        }

        bool valid = true;
        for (int i = 0; i < recipe.materialItemIds.Length; i++)
        {
            string id = recipe.materialItemIds[i]?.Trim();
            var target = ItemManager.Instance.GetItemBaseSO(id);
            if (target == null) valid = false;
            if (materialImages[i] != null) materialImages[i].sprite = target != null ? target.itemIcon : null;
            if (materialCountTexts[i] != null)
            {
                int count = model.Inventory.GetTotalItemCount(id);
                materialCountTexts[i].text = count + " / " + recipe.materialItemAmounts[i];
                materialCountTexts[i].color = count >= recipe.materialItemAmounts[i] ? Color.yellow : Color.red;
            }
        }

        string failure = null;
        bool canCraft = valid && NpcCraftManager.instance != null &&
            NpcCraftManager.instance.CanCraft(recipe, out failure);
        if (!canCraft) ShowFailure(failure ?? "제작 정보를 확인할 수 없습니다.");
        if (executeButton != null) executeButton.SetActive(canCraft);
    }

    private void ShowFailure(string message)
    {
        if (requireGoldAmountText != null)
        {
            requireGoldAmountText.text = message;
            requireGoldAmountText.color = Color.red;
        }
    }
}
