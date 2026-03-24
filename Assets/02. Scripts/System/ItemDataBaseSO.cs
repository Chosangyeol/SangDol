using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New ItemDataBaseSO", menuName = "System/ItemDataBaseSO")]
public class ItemDataBaseSO : ScriptableObject
{
    [Header("아이템 데이터베이스")]
    public List<ItemBaseSO> itemDataBase = new List<ItemBaseSO>();
    public List<EquipItemSO> equipItemDataBase = new List<EquipItemSO>();

#if UNITY_EDITOR
    [ContextMenu("CSV에서 아이템 SO 파일 자동 생성/갱신하기")]
    public void GenerateEquipItemSO()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("equipItemDataBase");

        if (csvFile == null)
        {
            Debug.LogError("리소스 폴더에 아이템 csv파일이 없습니다.");
            return;
        }

        string[] rows = csvFile.text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        equipItemDataBase.Clear();

        string splitRegex = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";

        for (int i = 1; i < rows.Length; i++)
        {
            string[] cols = Regex.Split(rows[i], splitRegex);

            if (cols.Length < 10) continue;

            for (int j = 0; j < cols.Length; j++)
            {
                cols[j] = cols[j].TrimStart('\"', ' ').TrimEnd('\"', ' ');
            }

            string id = cols[0];
            string name = cols[1];
            string nameKr = cols[2];

            EquipItemSO newItemSO = null;

            string assetPath = $"Assets/Resources/ItemSOs/EquipItems/{id}_{name}.asset";

            newItemSO = AssetDatabase.LoadAssetAtPath<EquipItemSO>(assetPath);

            if (newItemSO == null)
            {
                newItemSO = ScriptableObject.CreateInstance<EquipItemSO>();
                AssetDatabase.CreateAsset(newItemSO, assetPath);
            }

            newItemSO.itemID = id;
            newItemSO.itemName = nameKr;

            string iconPath = $"Assets/Resources/ItemIcons/{id}.png";
            newItemSO.itemIcon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            newItemSO.itemType = ItemEnums.ItemType.Equip;
            newItemSO.itemRarity = (ItemEnums.ItemRarity)Enum.Parse(typeof(ItemEnums.ItemRarity), cols[4]);
            newItemSO.itemDesc = cols[5];
            newItemSO.stackable = false;
            newItemSO.maxStack = 1;

            newItemSO.equipItemType = (ItemEnums.EquipItemType)Enum.Parse(typeof(ItemEnums.EquipItemType), cols[6]);
            newItemSO.statToIncrease = (C_Enums.CharacterStat)Enum.Parse(typeof(C_Enums.CharacterStat), cols[7]);

            string isflat = cols[8].ToUpper();
            newItemSO.isFlat = (isflat == "1" || isflat == "O" || isflat == "TRUE");
            newItemSO.value = float.Parse(cols[9]);

            EditorUtility.SetDirty(newItemSO);

            equipItemDataBase.Add(newItemSO);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(this);
    }
    #endif
}

