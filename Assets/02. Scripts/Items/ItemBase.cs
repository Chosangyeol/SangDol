using UnityEngine;

public abstract class ItemBase
{
    public ItemBaseSO itemBaseSO;
    public int maxStack = 99;
    public int currentStack = 1;

    public ItemBase(ItemBaseSO itemBaseSO, int currentStack)
    {
        this.itemBaseSO = itemBaseSO;
        this.maxStack = itemBaseSO.maxStack;
        this.currentStack = currentStack;
        return;
    }

    public abstract ItemBase Clone(int stack);

    /// <summary>
    /// 아이템이 인벤토리에 추가될때 호출되는 함수
    /// Inventory 클래스의 AddItem 함수에서 호출
    /// EX) 퀘스트 아이템이 인벤토리에 추가될때 호출
    /// </summary>
    public abstract void OnAddInventory();

    /// <summary>
    /// 인벤토리에 아이템이 있을 때 지속적으로 호출되는 함수
    /// Inventory 클래스의 UpdateItem 함수에서 호출
    /// EX) 일정 시간마다 체력을 회복하는 아이템
    /// </summary>
    public abstract void OnUpdateInventory(float delta);

    /// <summary>
    /// 아이템이 인벤토리에서 제거될때 호출되는 함수
    /// Inventory 클래스의 RemoveItem 함수에서 호출
    /// </summary>
    public abstract void OnRemoveInventory();
}
