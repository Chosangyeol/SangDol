using UnityEngine;

[CreateAssetMenu(fileName = "New UseItem", menuName = "Item/UseItemSO")]
public class UseItemSO : ItemBaseSO
{
    [Header("소비 아이템 세팅")]
    public ItemEnums.UseItemType useItemType;

    public bool isImmediately = false;
    public bool isPercent = true;
    public float effectAmount = 0f;
    public float itemDuration = 0f;

    public float coolDownTime = 0f;

    public BuffSO buffSO;

    public override ItemBase CreateItem(int stack)
    {
        switch (useItemType)
        {
            case ItemEnums.UseItemType.Heal:
                return new Heal(this, stack);
            case ItemEnums.UseItemType.AntiPoison:
                return new AntiPoison(this, stack);
                
        }

        return null;
    }

}
