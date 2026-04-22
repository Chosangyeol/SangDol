using UnityEngine;

public class EquipItemBase : ItemBase
{
    public new EquipItemSO itemBaseSO;
    public int currentUpgradeLevel = 0;

    public EquipItemBase(EquipItemSO itemBaseSO, int currentStack) : base(itemBaseSO, currentStack)
    {
        this.itemBaseSO = itemBaseSO;
        this.maxStack = 1;
        this.currentStack = 1;
        this.currentUpgradeLevel = 0;
        return;
    }

    public override ItemBase Clone(int stack)
    {
        EquipItemBase clone = new EquipItemBase(this.itemBaseSO, stack);
        // [핵심 2] 아이템이 인벤토리 내에서 위치를 옮기거나 복제될 때 강화 수치도 그대로 복사되게 처리
        clone.currentUpgradeLevel = this.currentUpgradeLevel;
        return clone;
    }

    public float GetFinalStat()
    {
        return itemBaseSO.value + (itemBaseSO.perUpgradeBonus * currentUpgradeLevel);
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
