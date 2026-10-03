using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("아이템 슬룻 정보")]
    public int slotIndex;

    [Header("UI 구성 요소")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject highlight;
    [SerializeField] private TMP_Text stackText;

    private Canvas rootCanvas;
    private Image dragIcon;
    private RectTransform dragIconRect;

    private ItemTooltip tooltip;

    private bool droppedOnSlot;
    private ItemBase draggedItem;

    private C_Inventory _inventory;
    private C_Equipment _equipment;

    private ItemBase currentItem =>
        _inventory != null && slotIndex >= 0 && slotIndex < _inventory.Items.Count ? _inventory.Items[slotIndex] : null;

    public ItemBase CurrentItem => currentItem;
    internal C_Inventory Inventory => _inventory;
    internal bool HasValidDrag => dragIcon != null && draggedItem != null && ReferenceEquals(draggedItem, currentItem);

    #region 생성 및 새로고침
    public void Init(C_Inventory inventory, C_Equipment equipment, int index,ItemTooltip tooltip)
    {
        _inventory = inventory;
        _equipment = equipment;
        slotIndex = index;
        this.tooltip = tooltip;


        rootCanvas = GetComponentInParent<Canvas>();
        Refresh();
    }

    public void Refresh()
    {
        ItemBase item = CurrentItem;

        if (item == null)
        {
            iconImage.enabled = false;
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

    #region 클릭
    public void OnPointerClick(PointerEventData eventData)
    {
        ItemBase item = CurrentItem;

        if (item == null) return;

        if (eventData.clickCount == 2)
        {
            if (item is EquipItemBase)
            {
                _equipment.EquipItem(item as EquipItemBase);
                if (tooltip != null) tooltip.ToggleTooltip(false,null,(ItemBase)null);
            }
            return;
        }
        Debug.Log($"슬롯 {slotIndex} 클릭됨");
    }

    #endregion

    #region 드래그 & 드랍
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CurrentItem == null || rootCanvas == null) return;

        droppedOnSlot = false;
        draggedItem = CurrentItem;

        dragIcon = new GameObject("DragIcon").AddComponent<Image>();
        dragIcon.transform.SetParent(rootCanvas.transform, false);
        dragIcon.raycastTarget = false;
        dragIcon.sprite = iconImage.sprite;

        dragIconRect = dragIcon.rectTransform;
        dragIconRect.sizeDelta = iconImage.rectTransform.sizeDelta;
        dragIconRect.position = eventData.position;
        iconImage.enabled = false;
    }

    private void OnEnable()
    {
        if (_inventory != null && iconImage != null && stackText != null) Refresh();
    }

    private void OnDisable()
    {
        if (dragIcon != null) Destroy(dragIcon.gameObject);
        dragIcon = null;
        dragIconRect = null;
        draggedItem = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconRect == null) return;

        dragIconRect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!droppedOnSlot)
            Debug.Log("아이템 버리기");

        if (dragIcon != null)
            Destroy(dragIcon.gameObject);

        dragIcon = null;
        dragIconRect = null;
        draggedItem = null;
        Refresh();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || _inventory == null || _equipment == null) return;
        InventorySlot fromInventorySlot = eventData.pointerDrag?.GetComponent<InventorySlot>();
        if (fromInventorySlot != null)
        {
            if (fromInventorySlot == this || fromInventorySlot.Inventory != _inventory ||
                !fromInventorySlot.HasValidDrag) return;
            if (!_inventory.TrySwap(fromInventorySlot.slotIndex, slotIndex)) return;
            fromInventorySlot.SetDropped(true);
            fromInventorySlot.Refresh();
            Refresh();
            return;
        }
        EquipmentSlot fromEquipmentSlot = eventData.pointerDrag?.GetComponent<EquipmentSlot>();
        if (fromEquipmentSlot == null || fromEquipmentSlot.Equipment != _equipment ||
            !fromEquipmentSlot.HasValidDrag) return;
        if (!_equipment.TryUnequipItem(fromEquipmentSlot.equipType, slotIndex)) return;
        fromEquipmentSlot.SetDropped(true);
        fromEquipmentSlot.Refresh();
        Refresh();
    }

    public void SetDropped(bool value)
    {
        droppedOnSlot = value;
    }
    #endregion

    #region 아이템 툴팁 출력
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem == null) return;
        if (tooltip == null) return;

        // tooltip 위치 지정
        tooltip.ToggleTooltip(true,this.GetComponent<RectTransform>(),currentItem);

        // tooltip에 아이템 정보 입력

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentItem == null) return;
        if (tooltip == null) return;

        if (tooltip != null) tooltip.ToggleTooltip(false,null,(ItemBase)null);

    }
    #endregion

    #region 인벤토리 액션
    private void Select(ItemBase item)
    {
        Debug.Log("아이템 선택");
        //highlight?.SetActive(true);
        // 아이템 정보 UI 호출
    }

    private void OpenContextMenu(ItemBase item)
    {
        // 컨텍스트 메뉴 UI 호출
        Debug.Log(item.itemBaseSO.itemName + " 컨텍스트 메뉴 열림");
    }
    #endregion
}
