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

        if (this.dropItemSO != null)
        {
            itemNameText.text = dropItemSO.itemName;
            dropItem = this.dropItemSO.CreateItem(amount);
        }
    }

    public override bool Interact(Transform target)
    {
        if (base.Interact(target))
        {
            // 아이템 획득 로직 추가
            Debug.Log($"아이템 '{InteractName}' 획득!");

            CharacterModel model = target.GetComponent<CharacterModel>();

            if (model != null )
            {
                model.Inventory.AddItem(dropItem);
            }
            Debug.Log($"아이템 '{dropItem.itemBaseSO.itemName}'이(가) 인벤토리에 추가되었습니다.");
            PoolManager.Instance.Push(this);
            return true;
        }

        return false;
    }
}
