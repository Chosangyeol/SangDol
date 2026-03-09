using UnityEngine;

[CreateAssetMenu(fileName = "New EquipItem", menuName = "Item/EquipItemSO")]
public class EquipItemSO : ItemBaseSO
{
    public ItemEnums.EquipItemType equipItemType;
    public C_Enums.CharacterStat statToIncrease;
    public bool isFlat = true;
    public float value;

    public override ItemBase CreateItem(int stack)
    {
        return new EquipItemBase(this, stack);
    }
}
