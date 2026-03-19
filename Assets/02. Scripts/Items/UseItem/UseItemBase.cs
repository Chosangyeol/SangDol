using UnityEngine;

public class UseItemBase : ItemBase
{
    public UseItemSO itemBaseSO;

    public UseItemBase(UseItemSO itemBaseSO, int currentStack) : base(itemBaseSO, currentStack)
    {
        this.itemBaseSO = itemBaseSO;
        this.currentStack = currentStack;
        return;
    }

    public override ItemBase Clone(int stack)
    {
        UseItemBase newItem = new UseItemBase(this.itemBaseSO,stack);
        newItem.currentStack = stack;
        return newItem;
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

    public bool UseItem(CharacterModel owner)
    {

    }
}
