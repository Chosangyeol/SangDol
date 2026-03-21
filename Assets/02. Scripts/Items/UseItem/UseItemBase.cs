using UnityEngine;

public abstract class UseItemBase : ItemBase
{
    public UseItemSO itemBaseSO;

    public UseItemBase(UseItemSO itemBaseSO, int currentStack) : base(itemBaseSO, currentStack)
    {
        this.itemBaseSO = itemBaseSO;
        this.currentStack = currentStack;
        return;
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

    public abstract bool UseItem(CharacterModel owner);
}
