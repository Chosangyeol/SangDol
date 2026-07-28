using UnityEngine;

[CreateAssetMenu(fileName = "New EquipItem", menuName = "Item/EquipItemSO")]
public class EquipItemSO : ItemBaseSO
{
    public ItemEnums.EquipItemType equipItemType;
    public C_Enums.CharacterStat statToIncrease;
    public bool isPercent = false;
    public float value;
    public bool canUpgrade = false;
    public int maxUpgrade = 0;
    public float perUpgradeBonus = 0;

    public UpgradeTableSO upgradeTable;

    public override ItemBase CreateItem(int stack)
    {
        return new EquipItemBase(this, stack);
    }
}
