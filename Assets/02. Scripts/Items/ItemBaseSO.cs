using UnityEngine;

[CreateAssetMenu(fileName = "New ItemSO", menuName = "Item/ItemSO")]
public abstract class ItemBaseSO : ScriptableObject
{
    [Header("아이템 고유 ID")]
    public int itemID;
    [Header("아이템 이름")]
    public string itemName;
    [Header("아이템 아이콘")]
    public Sprite itemIcon;
    [Header("아이템 종류")]
    public ItemEnums.ItemType itemType;
    [Header("아이템 등급")]
    public ItemEnums.ItemRarity itemRarity;
    [Header("아이템 설명")]
    public string itemDesc;
    [Header("아이템 중첩 가능 여부")]
    public bool stackable = true;
    [Header("아이템 최대 중첩 수")]
    public int maxStack = 99;

    public abstract ItemBase CreateItem(int stack);

}
