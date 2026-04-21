using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor.Searcher;



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
    public void GenerateItemSOFromCSV()
    {
        GenerateEquipItemSO();
        GenerateItemSO();
    }

    public ItemBaseSO GetItemByID(string itemID)
    {
        if (int.TryParse(itemID, out int idNumber))
        {
            if (idNumber >= 10000 && idNumber < 20000)
            {
                foreach (var equip in equipItemDataBase)
                {
                    if (equip.itemID == itemID)
                        return equip;
                }
            }
            else if (idNumber >= 20000 && idNumber < 30000)
            {
                foreach (var item in itemDataBase)
                {
                    if (item.itemID == itemID)
                        return item;
                }
            }
        }

        // 해당하는 범위를 찾지 못했거나, 리스트에 아이템이 없는 경우
        Debug.LogWarning($"데이터베이스에 ID가 '{itemID}'인 아이템을 찾을 수 없습니다! (ID 범위를 확인해주세요)");
        return null;
    }

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
            newItemSO.itemPrice = int.Parse(cols[6]);
            newItemSO.stackable = false;
            newItemSO.maxStack = 1;

            newItemSO.equipItemType = (ItemEnums.EquipItemType)Enum.Parse(typeof(ItemEnums.EquipItemType), cols[7]);
            newItemSO.statToIncrease = (C_Enums.CharacterStat)Enum.Parse(typeof(C_Enums.CharacterStat), cols[8]);

            string isflat = cols[9].ToUpper();
            newItemSO.isFlat = (isflat == "1" || isflat == "O" || isflat == "TRUE");
            newItemSO.value = float.Parse(cols[10]);

            EditorUtility.SetDirty(newItemSO);

            equipItemDataBase.Add(newItemSO);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(this);
    }

    public void GenerateItemSO()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("ItemDataBase");

        if (csvFile == null )
        {
            Debug.LogError("리소스 폴더에 아이템 csv파일이 없습니다.");
            return;
        }

        string[] rows = csvFile.text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        itemDataBase.Clear();

        string splitRegex = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";

        for (int i = 1; i < rows.Length; i++)
        {
            string[] cols = Regex.Split(rows[i], splitRegex);

            if (cols.Length < 8) continue;

            for (int j = 0; j < cols.Length; j++)
            {
                cols[j] = cols[j].TrimStart('\"', ' ').TrimEnd('\"', ' ');
            }

            string id = cols[0];
            string name = cols[1];
            string nameKr = cols[2];

            if (cols.Length > 8 && !string.IsNullOrWhiteSpace(cols[8]))
            {
                UseItemSO newItemSO = null;

                string assetPath = $"Assets/Resources/ItemSOs/UseItems/{id}_{name}.asset";

                newItemSO = AssetDatabase.LoadAssetAtPath<UseItemSO>(assetPath);

                if (newItemSO == null)
                {
                    newItemSO = ScriptableObject.CreateInstance<UseItemSO>();
                    AssetDatabase.CreateAsset(newItemSO, assetPath);
                }

                newItemSO.itemID = id;
                newItemSO.itemName = nameKr;

                string iconPath = $"Assets/Resources/ItemIcons/{id}.png";
                newItemSO.itemIcon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                newItemSO.itemType = ItemEnums.ItemType.Use;
                newItemSO.itemRarity = (ItemEnums.ItemRarity)Enum.Parse(typeof(ItemEnums.ItemRarity), cols[4]);
                newItemSO.itemDesc = cols[5];
                newItemSO.stackable = true;
                newItemSO.maxStack = int.Parse(cols[6]);
                newItemSO.itemPrice = int.Parse(cols[7]);

                newItemSO.useItemType = (ItemEnums.UseItemType)Enum.Parse(typeof(ItemEnums.UseItemType), cols[8]);
                newItemSO.effectedStat = (C_Enums.CharacterStat)Enum.Parse(typeof(C_Enums.CharacterStat), cols[9]);
                string isflat = cols[10].ToUpper();
                newItemSO.isFlat = (isflat == "1" || isflat == "O" || isflat == "TRUE");
                newItemSO.effectAmount = float.Parse(cols[11]);
                newItemSO.itemDuration = float.Parse(cols[12]);
                newItemSO.coolDownTime = float.Parse(cols[13]);

                EditorUtility.SetDirty(newItemSO);

                itemDataBase.Add(newItemSO);

            }
            else
            {
                NormalItemSO newItemSO = null;

                string assetPath = $"Assets/Resources/ItemSOs/NormalItems/{id}_{name}.asset";

                newItemSO = AssetDatabase.LoadAssetAtPath<NormalItemSO>(assetPath);

                if (newItemSO == null)
                {
                    newItemSO = ScriptableObject.CreateInstance<NormalItemSO>();
                    AssetDatabase.CreateAsset(newItemSO, assetPath);
                }

                newItemSO.itemID = id;
                newItemSO.itemName = nameKr;

                string iconPath = $"Assets/Resources/ItemIcons/{id}.png";
                newItemSO.itemIcon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                newItemSO.itemType = ItemEnums.ItemType.Normal;
                newItemSO.itemRarity = (ItemEnums.ItemRarity)Enum.Parse(typeof(ItemEnums.ItemRarity), cols[4]);
                newItemSO.itemDesc = cols[5];
                newItemSO.stackable = true;
                newItemSO.maxStack = int.Parse(cols[6]);
                newItemSO.itemPrice = int.Parse(cols[7]);

                EditorUtility.SetDirty(newItemSO);

                itemDataBase.Add(newItemSO);
            }
           
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(this);
    }
#endif
}

