using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropItemModel : InteractableObject
{
    public TMP_Text itemNameText;

    public ItemBaseSO dropItemSO;
    private ItemBase dropItem;

    public override void Init(string text)
    {
        base.Init(text);
    }

    public void InitItem(ItemBaseSO dropItemSO,int amount)
    {
        this.dropItemSO = dropItemSO;

        dropItem = this.dropItemSO != null ? this.dropItemSO.CreateItem(amount) : null;
        if (this.dropItemSO != null && itemNameText != null)
        {
            itemNameText.text = dropItemSO.itemName;
        }
    }

    public override bool Interact(Transform target)
    {
        if (!gameObject.activeInHierarchy || target == null || dropItem == null || dropItem.currentStack <= 0)
            return false;
        CharacterModel model = target.GetComponent<CharacterModel>();
        if (model == null || model.Inventory == null || !base.Interact(target)) return false;

        int received = model.Inventory.AddItemWithResult(dropItem);
        if (dropItem.currentStack > 0)
        {
            // Keep the uncollected remainder and restore its interaction guide.
            UpdateUIState();
            return received > 0;
        }

        if (PoolManager.Instance != null) PoolManager.Instance.Push(this);
        else gameObject.SetActive(false);
        return received > 0;
    }
}
