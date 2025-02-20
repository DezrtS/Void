using System;
using UnityEngine;

public class ItemRetrievalTask : Task
{
    public event Action<Item> OnItem;

    [SerializeField] private ItemData itemData;
    public ItemData ItemData => itemData;

    public void SpawnItem(Item item)
    {
        OnItem?.Invoke(item);
    }
}