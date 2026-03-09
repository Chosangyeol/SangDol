using UnityEngine;

[SerializeField]
public class Item : MonoBehaviour
{
    public ItemBaseSO _itemData;
    private ItemBase _item;
    public int stack;

    private void OnEnable()
    {
        _item = _itemData.CreateItem(stack);
    }

    public ItemBase GetItem()
    {
        return _item;
    }
}
