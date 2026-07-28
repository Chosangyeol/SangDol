using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CraftRecipe
{
    public int requireGoldAmount;
    public string[] materialItemIds;
    public int[] materialItemAmounts;
    public string resultItemId;
}

[CreateAssetMenu(fileName = "New CraftTableSO", menuName = "System/CraftTable")]
public class CraftTableSO : ScriptableObject
{
    public List<CraftRecipe> recipes;
}
