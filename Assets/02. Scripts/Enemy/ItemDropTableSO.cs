using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropItem
{
    public string itemID;
    [Range(0f, 100f)]
    public float dropPercent;
    public int amount;
}

[CreateAssetMenu(fileName = "New ItemDropTableSO", menuName = "SO/ItemDropTable")]
public class ItemDropTableSO : ScriptableObject
{
    public List<DropItem> dropItems;
}
