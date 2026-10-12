using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NpcCraftManager : MonoBehaviour
{
    public static NpcCraftManager instance;
    private CharacterModel _model;

    private CraftRecipe selectedRecipe = null;
    private CraftTableSO selectedTable = null;

    private List<CraftRecipe> craftRecipes = new List<CraftRecipe>();
    public List<CraftRecipe> CraftRecipes => craftRecipes;

    [Header("제작 UI")]
    public CraftUI craftPanel;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        _model = FindAnyObjectByType<CharacterModel>();
    }

    public void ListUpCraftRecipes(CraftTableSO table)
    {
        craftRecipes.Clear();

        selectedTable = null;

        selectedTable = table;

        if (selectedTable == null) return;

        foreach (var recipe in table.recipes)
        {
            craftRecipes.Add(recipe);
        }

        // UI에 띄우기
        craftPanel.SetCraftRecipeUI(CraftRecipes);
    }

    public void SelectCraftRecipe(CraftRecipe recipe)
    {
        selectedRecipe = null;

        selectedRecipe = recipe;

        if (selectedRecipe == null) return;

        // UI에 띄우기
        craftPanel.SetCraftExecuteUI(recipe, _model);
    }

    private bool crafting;

    private bool BuildCraftPlan(CraftRecipe recipe,
        out List<KeyValuePair<string, int>> materials,
        out List<KeyValuePair<ItemBaseSO, int>> rewards, out string failure)
    {
        materials = new List<KeyValuePair<string, int>>();
        rewards = new List<KeyValuePair<ItemBaseSO, int>>();
        failure = "제작 정보를 확인할 수 없습니다.";
        if (recipe == null || _model == null || _model.Inventory == null || _model.Stat == null ||
            _model.isDie || ItemManager.Instance == null || recipe.requireGoldAmount < 0 ||
            recipe.materialItemIds == null || recipe.materialItemAmounts == null ||
            recipe.materialItemIds.Length != recipe.materialItemAmounts.Length) return false;
        var totals = new Dictionary<string, int>();
        for (int i = 0; i < recipe.materialItemIds.Length; i++)
        {
            string id = recipe.materialItemIds[i]?.Trim();
            int amount = recipe.materialItemAmounts[i];
            if (string.IsNullOrEmpty(id) || amount <= 0) return false;
            totals.TryGetValue(id, out int previous);
            if ((long)previous + amount > int.MaxValue) return false;
            totals[id] = previous + amount;
        }
        if (_model.Stat.Stat.gold < recipe.requireGoldAmount)
        {
            failure = "제작에 필요한 골드가 부족합니다.";
            return false;
        }
        foreach (var pair in totals)
        {
            materials.Add(pair);
            if (_model.Inventory.GetTotalItemCount(pair.Key) < pair.Value)
            {
                failure = "제작에 필요한 재료가 부족합니다.";
                return false;
            }
        }
        var result = ItemManager.Instance.GetItemBaseSO(recipe.resultItemId);
        if (result == null) return false;
        rewards.Add(new KeyValuePair<ItemBaseSO, int>(result, 1));
        failure = null;
        return true;
    }

    internal bool CanCraft(CraftRecipe recipe, out string failure)
    {
        if (!BuildCraftPlan(recipe, out var materials, out var rewards, out failure)) return false;
        if (_model.Inventory.CanExchangeItems(materials, rewards)) return true;
        failure = "제작 결과를 받을 인벤토리 공간이 부족합니다.";
        return false;
    }

    public void ExecuteCraft()
    {
        if (crafting) return;
        var recipe = selectedRecipe;
        if (!BuildCraftPlan(recipe, out var materials, out var rewards, out var failure))
        {
            if (recipe != null) ShowCraftFailure(failure);
            return;
        }
        crafting = true;
        try
        {
            bool committed = _model.Inventory.TryExchangeItems(materials, rewards, () =>
            {
                // All inventory changes are visible before currency/events can reenter.
                selectedRecipe = null;
                _model.UseGold(recipe.requireGoldAmount);
            });
            if (!committed)
            {
                ShowCraftFailure("제작 결과를 받을 인벤토리 공간이 부족합니다.");
                return;
            }
            if (craftPanel != null) craftPanel.SetCraftExecuteUI(null, null);
        }
        finally { crafting = false; }
    }

    private void ShowCraftFailure(string message)
    {
        if (craftPanel != null)
        {
            if (craftPanel.executeButton != null) craftPanel.executeButton.SetActive(false);
            if (craftPanel.requireGoldAmountText != null)
            {
                craftPanel.requireGoldAmountText.text = message;
                craftPanel.requireGoldAmountText.color = Color.red;
            }
        }
        else Debug.LogWarning("[NpcCraftManager] " + message);
    }

    public void OpenUI(CraftTableSO tableSO)
    {
        craftPanel.gameObject.SetActive(true);
        craftPanel.SetCraftExecuteUI(null, null);
        ListUpCraftRecipes(tableSO);
    }

    public void CloseUI()
    {
        craftPanel.gameObject.SetActive(false);
    }
}
