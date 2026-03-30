using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NormalItem", menuName = "Item/NormalItemSO")]
public class NormalItemSO : ItemBaseSO
{
    public override ItemBase CreateItem(int stack)
    {
        return new NormalItemBase(this, stack);
    }
}
