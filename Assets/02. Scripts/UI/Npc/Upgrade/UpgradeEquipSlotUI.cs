using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeEquipSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("슬룻 UI")]
    public Image equipIcon;
    public TMP_Text itemName;
    public TMP_Text currentLevel;

    private EquipItemBase nowEquipItem = null;

    public void Init(EquipItemBase target)
    {
        nowEquipItem = target;

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
}
