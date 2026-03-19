using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltip : MonoBehaviour
{
    RectTransform rectTransform;

    public Vector2 offset = new Vector2(10f, 30f);

    [SerializeField] private TMP_Text _itemName;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _itemDesc;
    [SerializeField] private TMP_Text _itemRarity;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void ToggleTooltip(bool onoff, RectTransform owner = null, ItemBase item = null)
    {
        this.gameObject.SetActive(onoff);

        if (!onoff) return;

        Vector2 slotPos = owner.position;

        float pivotX = slotPos.x > Screen.width / 2f ? 1f : 0f;
        float pivotY = slotPos.y > Screen.height / 2f ? 1f : 0f;

        rectTransform.pivot = new Vector2(pivotX, pivotY);

        float offsetX = pivotX == 1f ? -offset.x : offset.x;
        float offsetY = pivotY == 1f ? -offset.y : offset.y;

        rectTransform.position = new Vector2(slotPos.x + offsetX, slotPos.y + offsetY);

        UpdateTooltip(item);
    }

    public void UpdateTooltip(ItemBase item)
    {
        if (item == null) return;

        _icon.sprite = item.itemBaseSO.itemIcon;

        _itemName.text = item.itemBaseSO.itemName;
        _itemRarity.text = item.itemBaseSO.itemRarity.ToString();
        _itemDesc.text = item.itemBaseSO.itemDesc;


        if (item is not EquipItemBase equip) return;

        Debug.Log("장비 아이템");
    }
}
