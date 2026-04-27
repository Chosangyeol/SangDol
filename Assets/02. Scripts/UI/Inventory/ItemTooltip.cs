using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class ItemTooltip : MonoBehaviour
{
    RectTransform rectTransform;

    public Vector2 offset = new Vector2(10f, 30f);

    [Header("공통 아이템 UI")]
    [SerializeField] private TMP_Text _itemName;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _itemRarity;
    [SerializeField] private TMP_Text _itemPrice;
    [SerializeField] private TMP_Text _itemDesc;

    [Header("장비 아이템 UI")]
    [SerializeField] private GameObject _equipPanel;
    [SerializeField] private TMP_Text _equipDefaultStat;
    [SerializeField] private TMP_Text _equipUpgradeStat;

    private CanvasGroup canvasGroup;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ToggleTooltip(bool onoff, RectTransform owner = null, ItemBase item = null)
    {
        if (!onoff)
        {
            this.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;

        StartCoroutine(TooltipCo(owner, item));
    }

    private IEnumerator TooltipCo(RectTransform owner = null, ItemBase item = null)
    {
        

        Vector2 slotPos = owner.position;

        float pivotX = slotPos.x > Screen.width / 2f ? 1f : 0f;
        float pivotY = slotPos.y > Screen.height / 2f ? 1f : 0f;

        rectTransform.pivot = new Vector2(pivotX, pivotY);

        float offsetX = pivotX == 1f ? -offset.x : offset.x;
        float offsetY = pivotY == 1f ? -offset.y : offset.y;

        rectTransform.position = new Vector2(slotPos.x + offsetX, slotPos.y + offsetY);

        UpdateTooltip(item);

        yield return null;

        canvasGroup.alpha = 1f;
    }

    public void ToggleTooltip(bool onoff, RectTransform owner = null, ItemBaseSO itemSO = null)
    {

        if (!onoff)
        {
            this.gameObject.SetActive(false);
            return;
        }

        this.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;

        StartCoroutine(TooltipCo(owner, itemSO));
    }

    private IEnumerator TooltipCo(RectTransform owner = null, ItemBaseSO itemSO = null)
    {

        Vector2 slotPos = owner.position;

        float pivotX = slotPos.x > Screen.width / 2f ? 1f : 0f;
        float pivotY = slotPos.y > Screen.height / 2f ? 1f : 0f;

        rectTransform.pivot = new Vector2(pivotX, pivotY);

        float offsetX = pivotX == 1f ? -offset.x : offset.x;
        float offsetY = pivotY == 1f ? -offset.y : offset.y;

        rectTransform.position = new Vector2(slotPos.x + offsetX, slotPos.y + offsetY);

        UpdateTooltip(itemSO);

        yield return null;

        canvasGroup.alpha = 1f;
    }

    public void UpdateTooltip(ItemBase item)
    {
        if (item == null) return;

        _icon.sprite = item.itemBaseSO.itemIcon;
        _itemPrice.text = $"{item.itemBaseSO.itemPrice}G";
        _itemDesc.text = item.itemBaseSO.itemDesc;

        if (item is not EquipItemBase equip)
        {
            _equipPanel.SetActive(false);
            _itemName.text = $"{item.itemBaseSO.itemName} X {item.currentStack}";
            _itemRarity.text = $"{ItemManager.Instance.ReturnRarity(item.itemBaseSO.itemRarity)} 등급 아이템";
            
        }
        else
        {
            _equipPanel.SetActive(true);

            string finalValue = "";

            if (equip.itemBaseSO.statToIncrease == C_Enums.CharacterStat.CriticalChance
                || equip.itemBaseSO.statToIncrease == C_Enums.CharacterStat.CriticalDamage
                || equip.itemBaseSO.statToIncrease == C_Enums.CharacterStat.AttackSpeed
                || equip.itemBaseSO.statToIncrease == C_Enums.CharacterStat.MoveSpeed
                || equip.itemBaseSO.isPercent)
            {
                finalValue = $"{equip.itemBaseSO.value * 100}%";
            }
            else
                finalValue = $"{equip.itemBaseSO.value}";

            _itemName.text = $"{equip.itemBaseSO.itemName}";
            _itemRarity.text = $"{ItemManager.Instance.ReturnRarity(equip.itemBaseSO.itemRarity)} 등급 {ItemManager.Instance.ReturnEquipType(equip.itemBaseSO.equipItemType)}";
            _equipDefaultStat.text = $"<color=#1ABEFF>추가 스탯</color>\n" +
                $"{ItemManager.Instance.ReturnEffectStatType(equip.itemBaseSO.statToIncrease)} + {finalValue}";

            if (equip.itemBaseSO.statToIncrease == C_Enums.CharacterStat.CriticalChance
                || equip.itemBaseSO.statToIncrease == C_Enums.CharacterStat.CriticalDamage
                || equip.itemBaseSO.statToIncrease == C_Enums.CharacterStat.AttackSpeed
                || equip.itemBaseSO.statToIncrease == C_Enums.CharacterStat.MoveSpeed)
            {
                finalValue = $"{equip.itemBaseSO.perUpgradeBonus * equip.currentUpgradeLevel * 100}%";
            }
            else
                finalValue = $"{equip.itemBaseSO.perUpgradeBonus * equip.currentUpgradeLevel}";

            if (!equip.itemBaseSO.canUpgrade)
                _equipUpgradeStat.text = "<color=red>강화 불가능</color>";
            else
            {
                if (equip.currentUpgradeLevel < 1)
                {
                    _equipUpgradeStat.text = $"<color=green>강화 가능</color>";
                }
                else
                {
                    _equipUpgradeStat.text = $"<color=#1ABEFF>강화 효과</color>\n" +
                        $"{ItemManager.Instance.ReturnEffectStatType(equip.itemBaseSO.statToIncrease)} + {finalValue}";
                }
            }
        }
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        Debug.Log("장비 아이템");
    }

    public void UpdateTooltip(ItemBaseSO itemSO)
    {
        if (itemSO == null) return;

        _icon.sprite = itemSO.itemIcon;
        _itemPrice.text = $"{itemSO.itemPrice}G";
        _itemDesc.text = itemSO.itemDesc;
        _itemName.text = $"{itemSO.itemName}";

        if (itemSO is not EquipItemSO equip)
        {
            _equipPanel.SetActive(false);
            
            _itemRarity.text = $"{ItemManager.Instance.ReturnRarity(itemSO.itemRarity)} 등급 아이템";

        }
        else
        {
            _equipPanel.SetActive(true);

            string finalValue = "";

            if (equip.statToIncrease == C_Enums.CharacterStat.CriticalChance
                || equip.statToIncrease == C_Enums.CharacterStat.CriticalDamage
                || equip.statToIncrease == C_Enums.CharacterStat.AttackSpeed
                || equip.statToIncrease == C_Enums.CharacterStat.MoveSpeed
                || equip.isPercent)
            {
                finalValue = $"{equip.value * 100}%";
            }
            else
                finalValue = $"{equip.value}";

            _itemRarity.text = $"{ItemManager.Instance.ReturnRarity(equip.itemRarity)} 등급 {ItemManager.Instance.ReturnEquipType(equip.equipItemType)}";
            _equipDefaultStat.text = $"<color=#1ABEFF>추가 스탯</color>\n" +
                $"{ItemManager.Instance.ReturnEffectStatType(equip.statToIncrease)} + {finalValue}";

            if (!equip.canUpgrade)
                _equipUpgradeStat.text = "<color=red>강화 불가능</color>";
            else
            {
                _equipUpgradeStat.text = $"<color=green>강화 가능</color>";
            }
        }
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        Debug.Log("장비 아이템");
    }
}
