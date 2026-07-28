using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeEquipSlotUI : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("슬룻 UI")]
    public Image equipIcon;
    public TMP_Text itemName;
    public TMP_Text currentLevel;

    private EquipItemBase nowEquipItem = null;
    private ItemTooltip tooltip;

    public void Init(EquipItemBase target, ItemTooltip tooltip)
    {
        nowEquipItem = target;
        this.tooltip = tooltip;

        equipIcon.sprite = target.itemBaseSO.itemIcon;
        itemName.text = target.itemBaseSO.itemName;
        currentLevel.text = $"{target.currentUpgradeLevel} 단계";
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log($"<color=cyan>[{nowEquipItem.itemBaseSO.itemName}] 슬롯 클릭됨!</color>"); // 이거 추가!
            NpcUpgradeManager.instance.SelectUpgradeTarget(nowEquipItem);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (nowEquipItem == null) return;
        if (tooltip == null) return;

        tooltip.ToggleTooltip(true, this.GetComponent<RectTransform>(), nowEquipItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (nowEquipItem == null) return;
        if (tooltip == null) return;

        tooltip.ToggleTooltip(false,null,(ItemBase)null);
    }
}
