using UnityEngine;

public class EquipItemBase : ItemBase
{
    public EquipItemSO itemBaseSO;

    public EquipItemBase(EquipItemSO itemBaseSO, int currentStack) : base(itemBaseSO, currentStack)
    {
        this.itemBaseSO = itemBaseSO;
        this.maxStack = 1;
        this.currentStack = 1;
        return;
    }

    public override ItemBase Clone(int stack)
    {
        return new EquipItemBase(this.itemBaseSO, stack);
    }

    public override void OnAddInventory()
    {
        Debug.Log("인벤토리 추가 : " + itemBaseSO.itemName);
    }

    public override void OnUpdateInventory(float delta)
    {
        
    }

    public override void OnRemoveInventory()
    {
        
    }

}
