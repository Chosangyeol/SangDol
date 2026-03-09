using UnityEngine;

[CreateAssetMenu(fileName = "New UseItem", menuName = "Item/UseItemSO")]
public class UseItemSO : ItemBaseSO
{
    public override ItemBase CreateItem(int stack)
    {
        return new UseItemBase(this, stack);
    }
}
