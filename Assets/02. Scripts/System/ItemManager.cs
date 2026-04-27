using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    public ItemDataBaseSO itemDataBaseSO;

    private Dictionary<string, ItemBaseSO> itemBaseDic = new Dictionary<string, ItemBaseSO>();
    private Dictionary<string, EquipItemSO> equipItemDic = new Dictionary<string, EquipItemSO>();
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitItemDataBase();
    }

    private void InitItemDataBase()
    {
        foreach (var itemBase in itemDataBaseSO.itemDataBase)
        {
            if (!itemBaseDic.ContainsKey(itemBase.itemID))
            {
                itemBaseDic.Add(itemBase.itemID, itemBase);
            }
        }

        foreach (var equipItem in itemDataBaseSO.equipItemDataBase)
        {
            if (!equipItemDic.ContainsKey(equipItem.itemID))
            {
                equipItemDic.Add(equipItem.itemID, equipItem);
            }
        }
    }

    public ItemBaseSO GetItemBaseSO(string itemID)
    {
        // 1. 잘못된 값 방지
        if (string.IsNullOrEmpty(itemID)) return null;

        // 2. ID를 숫자로 변환하여 대역폭 확인
        if (int.TryParse(itemID, out int idNumber))
        {
            // 3. 10000번대 (장비 아이템)
            if (idNumber >= 10000 && idNumber < 20000)
            {
                // 딕셔너리에서 빠르게 검색
                if (equipItemDic.TryGetValue(itemID, out EquipItemSO equipItem))
                {
                    // EquipItemSO는 ItemBaseSO를 상속받았으므로 자동 형변환되어 반환됩니다.
                    return equipItem;
                }
            }
            // 4. 20000번대 (일반/소비 아이템)
            else if (idNumber >= 20000 && idNumber < 30000)
            {
                if (itemBaseDic.TryGetValue(itemID, out ItemBaseSO normalItem))
                {
                    return normalItem;
                }
            }
        }

        // 5. ID가 범위를 벗어났거나 딕셔너리에 없을 경우 예외 처리
        Debug.LogWarning($"[ItemManager] 해당 ID({itemID})를 가진 아이템을 찾을 수 없습니다.");
        return null;
    }

    public string ReturnRarity(ItemEnums.ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemEnums.ItemRarity.Common: return "<color=grey>일반</color>";
            case ItemEnums.ItemRarity.Uncommon: return "<color=green>고급</color>";
            case ItemEnums.ItemRarity.Rare: return "<color=#1ABEFF>희귀</color>";
            case ItemEnums.ItemRarity.Unique: return "<color=purple>유니크</color>";
            case ItemEnums.ItemRarity.Legendary: return "<color=yellow>전설</color>";
        }
        return "Unknown";
    }

    public string ReturnEquipType(ItemEnums.EquipItemType type)
    {
        switch (type)
        {
            case ItemEnums.EquipItemType.Head: return "투구";
            case ItemEnums.EquipItemType.Body: return "상의";
            case ItemEnums.EquipItemType.Gloves: return "장갑";
            case ItemEnums.EquipItemType.Pants: return "하의";
            case ItemEnums.EquipItemType.Shoes: return "신발";
            case ItemEnums.EquipItemType.Weapon: return "무기";
        }
        return "Unknown";
    }

    public string ReturnEffectStatType(C_Enums.CharacterStat type)
    {
        switch (type)
        {
            case C_Enums.CharacterStat.MaxHp: return "최대 체력";
            case C_Enums.CharacterStat.AttackDamage: return "공격력";
            case C_Enums.CharacterStat.MoveSpeed: return "이동속도";
            case C_Enums.CharacterStat.AttackSpeed: return "공격속도";
            case C_Enums.CharacterStat.CriticalChance: return "치명타 확룰";
            case C_Enums.CharacterStat.CriticalDamage: return "치명타 피해";
            case C_Enums.CharacterStat.DownPower: return "무력화 피해";
            case C_Enums.CharacterStat.CooldownReduction: return "스킬 쿨타임 감소";
            case C_Enums.CharacterStat.DamageTakeMultiplier: return "받는 피해 감소";
            default: return "Unknown";
        }
    }
}
