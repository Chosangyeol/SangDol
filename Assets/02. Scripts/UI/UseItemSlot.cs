using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UseItemSlot : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [Header("아이템 슬룻 설정")]
    public C_Enums.UseSlot useSlot;

    private int rinkedSlotIndex = 99;

    [Header("UI 구성 요소")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject highlight;
    [SerializeField] private TMP_Text stackText;

    private Canvas rootCanvas;
    private Image dragIcon;
    private RectTransform dragIconRect;

    private ItemTooltip tooltip;

    private bool droppedOnSlot;

    private C_Inventory _inventory;

    private ItemBase currentItem =>
        rinkedSlotIndex < _inventory.Items.Count ? _inventory.Items[rinkedSlotIndex] : null;
    public ItemBase CurrentItem => currentItem;

    #region 생성 및 새로고침
    public void Init(C_Inventory inventory, ItemTooltip tooltip)
    {
        _inventory = inventory;
        this.tooltip = tooltip;

        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void Refresh()
    {
        ItemBase item = CurrentItem;

        if (item == null)
        {
            stackText.enabled = false;
            stackText.text = "";
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = item.itemBaseSO.itemIcon;

        if (item.itemBaseSO.stackable && item.currentStack > 1)
        {
            stackText.enabled = true;
            stackText.text = item.currentStack.ToString();
        }
        else
        {
            stackText.enabled = false;
            stackText.text = "";
        }
    }
    #endregion

    #region 드래그 & 드랍
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CurrentItem == null) return;

        droppedOnSlot = false;

        dragIcon = new GameObject("DragIcon").AddComponent<Image>();
        dragIcon.transform.SetParent(rootCanvas.transform, false);
        dragIcon.raycastTarget = false;
        dragIcon.sprite = iconImage.sprite;

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
        if (!droppedOnSlot) rinkedSlotIndex = -1;

        if (dragIcon != null)
            Destroy(dragIcon.gameObject);

        dragIcon = null;
        dragIconRect = null;
        Refresh();
    }

    public void OnDrop(PointerEventData eventData)
    {
        UseItemSlot fromUseItemSlot = 
            eventData.pointerDrag?.GetComponent<UseItemSlot>();

        if (fromUseItemSlot != null)
        {
            fromUseItemSlot.SetDropped(true);

            int temp = rinkedSlotIndex;
            rinkedSlotIndex = fromUseItemSlot.rinkedSlotIndex;
            fromUseItemSlot.rinkedSlotIndex = temp;

            Refresh();
        }

        InventorySlot fromInventorySlot =
            eventData.pointerDrag?.GetComponent<InventorySlot>();

        if (fromInventorySlot != null && _inventory.Items[fromInventorySlot.slotIndex] is UseItemBase)
        {
            fromInventorySlot.SetDropped(true);

            rinkedSlotIndex = fromInventorySlot.slotIndex;

            Refresh();
        }
    }

    public void SetDropped(bool value)
    {
        droppedOnSlot = value;
    }
    #endregion

}
