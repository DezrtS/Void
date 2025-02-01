using System;
using UnityEngine;

public class ItemDropOff : MonoBehaviour, IInteractable
{
    public delegate void ItemDropOffHandler(Item item, ItemDropOff itemDropOff);
    public static event ItemDropOffHandler OnDropOff;

    public void Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out SurvivorController survivorController))
        {
            Item item = survivorController.Hotbar.GetActiveItem();
            if (item != null)
            {
                OnDropOff?.Invoke(item, this);
                survivorController.Hotbar.DropItem();
                item.transform.position = transform.position; 
            }
        }
    }
}
