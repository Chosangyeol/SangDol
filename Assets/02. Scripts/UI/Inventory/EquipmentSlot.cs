using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

[SerializeField]
public class EquipmentSlot : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [Header("장비 슬롯 정보")]
    public ItemEnums.EquipItemType equipType;

    [Header("UI 구성 요소")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject highlight;

    private bool droppedOnSlot;

    private C_Equipment _equipment;

    private EquipItemBase equipItem =>
        _equipment.equipItems.ContainsKey(equipType) ? _equipment.equipItems[equipType] : null;

    public EquipItemBase EquipItem => equipItem;

    private Canvas rootCanvas;
    private Image dragIcon;
    private RectTransform dragIconRect;

    public void Init(C_Equipment equipment, ItemEnums.EquipItemType equipType)
    {
        _equipment = equipment;
        this.equipType = equipType;

        rootCanvas = GetComponentInParent<Canvas>();
        Refresh();
    }

    public void Refresh()
    {
        EquipItemBase item = equipItem;

        if (item == null)
        {
            iconImage.enabled = false;
            return;
        }
        else
        {
            iconImage.enabled = true;
            iconImage.sprite = item.itemBaseSO.itemIcon;
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        ItemBase item = equipItem;

        if (item == null) return;

        if (eventData.clickCount == 2)
        {
            _equipment.UnequipItem(equipType);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("좌클릭 ; " + item.itemBaseSO.itemName);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("우클릭 : " + item.itemBaseSO.itemName);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (equipItem == null) return;

        droppedOnSlot = false;

        dragIcon = new GameObject("DragIcon").AddComponent<Image>();
        dragIcon.transform.SetParent(rootCanvas.transform, false);
        dragIcon.raycastTarget = false;
        dragIcon.sprite = equipItem.itemBaseSO.itemIcon;

        dragIconRect = dragIcon.rectTransform;
        dragIconRect.sizeDelta = iconImage.rectTransform.sizeDelta;
        dragIconRect.position = eventData.position;
        iconImage.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconRect == null) return;

        dragIconRect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            Destroy(dragIcon.gameObject);

        dragIcon = null;
        dragIconRect = null;
        Refresh();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot fromSlot =
            eventData.pointerDrag?.GetComponent<InventorySlot>();

        if (fromSlot == null) return;

        if (fromSlot.CurrentItem is not EquipItemBase draggedEquipItem)
            return;

        if (draggedEquipItem.itemBaseSO.equipItemType != equipType)
            return;

        fromSlot.SetDropped(true);

        _equipment.EquipItem(draggedEquipItem);

        fromSlot.Refresh();
        Refresh();
    }

    public void SetDropped(bool value)
    {
        droppedOnSlot = value;
    }
}

