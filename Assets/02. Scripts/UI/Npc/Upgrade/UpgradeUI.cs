using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("강화 가능 아이템 목록 UI")]
    public GameObject gridLayout;
    public List<GameObject> slots = new List<GameObject>();
    public GameObject slotUI;

    [Header("강화 실행 UI")]
    public Image targetEquipIcon;
    public TMP_Text targetEquipNameText;
    public TMP_Text levelText;
    public Image requireGoldImage;
    public TMP_Text requireGoldAmountText;
    public Image[] materialImages;
    public TMP_Text[] materialCountTexts;
    public GameObject executeButton;

    public void SetEquipListUI(List<EquipItemBase> itemList)
    {
        for (int n = 0; n < slots.Count; n++)
            Destroy(slots[n]);

        slots.Clear();

        for (int i = 0; i < itemList.Count; i++)
        {
            GameObject slot = Instantiate(slotUI, gridLayout.transform);
            slot.GetComponent<UpgradeEquipSlotUI>().Init(itemList[i]);
            slots.Add(slot);
        }
    }

    public void SetUpgradeUI(EquipItemBase target,UpgradeData upgradeData, CharacterModel model)
    {
        if (target == null)
        {
            targetEquipIcon.sprite = null;
            targetEquipNameText.text = "";
            levelText.text = "";

            requireGoldAmountText.text = "";

            for (int i = 0; i < 4; i++)
            { 
                materialCountTexts[i].text = "";
                materialImages[i].sprite = null;
            }
            executeButton.SetActive(false);

            return;
        }

        C_Inventory inventory = model.Inventory;

        bool canUpgrade = true;

        targetEquipIcon.sprite = target.itemBaseSO.itemIcon;
        targetEquipNameText.text = target.itemBaseSO.itemName;
        levelText.text = $"{target.currentUpgradeLevel}단계 -> {target.currentUpgradeLevel + 1}단계";

        requireGoldAmountText.text = $"{model.Stat.Stat.gold} / {upgradeData.requireGold}";

        if (model.Stat.Stat.gold < upgradeData.requireGold)
        {
            canUpgrade = false;
            requireGoldAmountText.color = Color.red;
        }
        else
            requireGoldAmountText.color = Color.yellow;

        int index = 0;

        for (int i = 0; i < upgradeData.requireMaterialID.Length; i++)
        {
            string targetID = upgradeData.requireMaterialID[i];

            materialImages[i].sprite = ItemManager.Instance.GetItemBaseSO(targetID).itemIcon;
            materialCountTexts[i].text =
                $"{inventory.GetTotalItemCount(targetID)} / {upgradeData.requiredMaterialAmount[i]}";

            if (inventory.GetTotalItemCount(targetID) < upgradeData.requiredMaterialAmount[i])
            {
                canUpgrade = false;
                materialCountTexts[i].color = Color.red;
            }
            else
                materialCountTexts[i].color = Color.yellow;

            index++;
        }

        for (int i = index; i < upgradeData.requireMaterialID.Length; i++)
        {
            materialCountTexts[i].text = "";
            materialImages[i].sprite = null;
        }
        executeButton.SetActive(canUpgrade);
    }
}
