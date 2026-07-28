using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalItemBase : ItemBase
{
    public NormalItemSO itemBaseSO;

    public NormalItemBase(NormalItemSO itemBaseSO, int currentStack) : base(itemBaseSO, currentStack)
    {
        this.itemBaseSO = itemBaseSO;
        this.currentStack = 1;
        return;
    }

    public override ItemBase Clone(int stack)
    {
        return new NormalItemBase(itemBaseSO, stack);
    }

    public override void OnAddInventory()
    {
        Debug.Log("인벤토리 추가 : " + itemBaseSO.itemName);
        GameEvent.OnGetItem?.Invoke(itemBaseSO.itemID);
    }

    public override void OnUpdateInventory(float delta)
    {

    }

    public override void OnRemoveInventory()
    {

    }
}
