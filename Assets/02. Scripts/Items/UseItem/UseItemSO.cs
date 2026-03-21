using UnityEngine;

[CreateAssetMenu(fileName = "New UseItem", menuName = "Item/UseItemSO")]
public class UseItemSO : ItemBaseSO
{
    [Header("소비 아이템 세팅")]
    public ItemEnums.UseItemType useItemType;
    public C_Enums.CharacterStat effectedStat = C_Enums.CharacterStat.None;
    
    public float effectAmount = 0f;
    public float itemDuration = 0f;

    public float coolDownTime = 0f;

    public override ItemBase CreateItem(int stack)
    {
        switch (useItemType)
        {
            case ItemEnums.UseItemType.Potion:
                return new Potion(this, stack);
            case ItemEnums.UseItemType.Scroll:
                break;
                
        }

        return null;
    }

}
