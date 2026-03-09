using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
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

    private bool droppedOnSlot;

    private C_Inventory _inventory;
    private C_Equipment _equipment;

    private ItemBase currentItem =>
        slotIndex < _inventory.Items.Count ? _inventory.Items[slotIndex] : null;

    public ItemBase CurrentItem => currentItem;

    #region 생성 및 새로고침
    public void Init(C_Inventory inventory, C_Equipment equipment, int index)
    {
        _inventory = inventory;
        _equipment = equipment;
        slotIndex = index;

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
            }
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Select(item);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OpenContextMenu(item);
        }
        Debug.Log($"슬롯 {slotIndex} 클릭됨");
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
        if (!droppedOnSlot)
            Debug.Log("아이템 버리기");

        if (dragIcon != null)
            Destroy(dragIcon.gameObject);

        dragIcon = null;
        dragIconRect = null;
        Refresh();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot fromInventorySlot =
            eventData.pointerDrag?.GetComponent<InventorySlot>();

        if (fromInventorySlot != null)
        {
            if (fromInventorySlot == this) return;

            fromInventorySlot.SetDropped(true);

            _inventory.Swap(fromInventorySlot.slotIndex, slotIndex);
            fromInventorySlot.Refresh();
            Refresh();
            return;
        }

        EquipmentSlot fromEquipmentSlot =
            eventData.pointerDrag?.GetComponent<EquipmentSlot>();

        if (fromEquipmentSlot != null)
        {
            EquipItemBase equipItem = fromEquipmentSlot.EquipItem;
            if (equipItem == null) return;

            fromEquipmentSlot.SetDropped(true);

            // 여기서 장비 해제 처리
            _equipment.UnequipItem(fromEquipmentSlot.equipType, this.slotIndex);

            fromEquipmentSlot.Refresh();
            Refresh();
        }
    }

    public void SetDropped(bool value)
    {
        droppedOnSlot = value;
    }
    #endregion

    #region 인벤토리 액션
    private void Select(ItemBase item)
    {
        Debug.Log("아이템 선택");
        //highlight?.SetActive(true);
        // 아이템 정보 UI 호출
    }

    private void Use(ItemBase item)
    {
        // 아이템 사용 로직
        Debug.Log(item.itemBaseSO.itemName + " 사용됨");
    }

    private void OpenContextMenu(ItemBase item)
    {
        // 컨텍스트 메뉴 UI 호출
        Debug.Log(item.itemBaseSO.itemName + " 컨텍스트 메뉴 열림");
    }
    #endregion
}
