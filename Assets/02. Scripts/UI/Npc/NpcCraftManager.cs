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

    public void ExecuteCraft()
    {
        if (selectedRecipe == null) return;

        _model.UseGold(selectedRecipe.requireGoldAmount);

        for (int i = 0; i < selectedRecipe.materialItemIds.Length; i++)
        {
            _model.Inventory.RemoveTargetItem(selectedRecipe.materialItemIds[i], selectedRecipe.materialItemAmounts[i]);
        }

        ItemBaseSO result = ItemManager.Instance.GetItemBaseSO(selectedRecipe.resultItemId);

        _model.Inventory.AddItem(result.CreateItem(1));

        SelectCraftRecipe(null);
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
