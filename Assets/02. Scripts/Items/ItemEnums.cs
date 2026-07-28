using UnityEngine;

public class ItemEnums
{
    /// <summary>
    /// 전체적인 아이템의 종류 ( 장비, 소비, 퀘스트 등 )
    /// </summary>
    public enum ItemType
    {
        Equip,
        Normal,
        Use
    }

    /// <summary>
    /// 아이템의 등급 ( 일반, 희귀, 영웅 등 )
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Unique,
        Legendary
    }

    /// <summary>
    /// 장비 아이템의 착용 부위 ( 머리, 몸통, 다리 등 )
    /// </summary>
    public enum EquipItemType
    {
        Head,
        Body,
        Pants,
        Gloves,
        Shoes,
        Weapon
    }

    public enum UseItemType
    {
        Heal,
        AntiPoison,
        AntiFear,
        Active
    }
}
