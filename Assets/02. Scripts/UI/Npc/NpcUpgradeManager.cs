using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class NpcUpgradeManager : MonoBehaviour
{
    public static NpcUpgradeManager instance;
    private CharacterModel _model;

    private EquipItemBase selectedEquip = null;

    private List<EquipItemBase> canUpgradeEquips = new List<EquipItemBase>();
    public List<EquipItemBase> CanUpgradeEquips => canUpgradeEquips;

    [Header("강화 UI")]
    public UpgradeUI upgradePanel;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        _model = FindAnyObjectByType<CharacterModel>();
    }

    public void FindCanUpgradeEquips()
    {
        canUpgradeEquips.Clear();

        C_Inventory inventory = _model.Inventory;

        if (inventory == null) return;

        foreach (var item in inventory.Items)
        {
            if (item is EquipItemBase equip && equip.itemBaseSO.upgradeTable != null
                && equip.itemBaseSO.canUpgrade && equip.currentUpgradeLevel < equip.itemBaseSO.maxUpgrade)
                canUpgradeEquips.Add(equip);
        }

        upgradePanel.SetEquipListUI(CanUpgradeEquips);
    }

    public void SelectUpgradeTarget(EquipItemBase target)
    {
        selectedEquip = target;

        if (selectedEquip == null) return;

        UpgradeTableSO table = selectedEquip.itemBaseSO.upgradeTable;

        if (table == null) return;

        UpgradeData nextUpgradeDate = table.GetUpgradeData(selectedEquip.currentUpgradeLevel);

        if (nextUpgradeDate == null) return;

        upgradePanel.SetUpgradeUI(target, nextUpgradeDate, _model);
    }

    public void ExecuteUpgrade()
    {
        if (selectedEquip == null) return;

        UpgradeTableSO table = selectedEquip.itemBaseSO.upgradeTable;
        UpgradeData upgradeData = table.GetUpgradeData(selectedEquip.currentUpgradeLevel);

        if (upgradeData == null) return;

        _model.UseGold(upgradeData.requireGold);

        for (int i = 0; i < upgradeData.requireMaterialID.Length; i++)
        {
            _model.Inventory.RemoveTargetItem(upgradeData.requireMaterialID[i], upgradeData.requiredMaterialAmount[i]);
        }

        float randomRoll = Random.Range(0f, 100f);

        if (randomRoll <= upgradeData.successRate)
        {
            // 성공 연출
            selectedEquip.currentUpgradeLevel++;

            if (selectedEquip.currentUpgradeLevel == selectedEquip.itemBaseSO.maxUpgrade)
            {
                selectedEquip = null;
                upgradePanel.SetUpgradeUI(selectedEquip, null, null);
            }
        }
        else
        {
            if (upgradeData.breakable)
            {
                // 파괴 연출
                _model.Inventory.RemoveItem(selectedEquip);
                selectedEquip = null;
                upgradePanel.SetUpgradeUI(selectedEquip,null,null);
            }
            else
            {
                // 실패 연출
            }
        }

        FindCanUpgradeEquips();

        if (selectedEquip != null)
            SelectUpgradeTarget(selectedEquip);

    }

    public void OpenUI()
    {
        upgradePanel.gameObject.SetActive(true);
        upgradePanel.SetUpgradeUI(null, null, null);
        FindCanUpgradeEquips();
    }

    public void CloseUI()
    {
        upgradePanel.gameObject.SetActive(false);
    }
}
