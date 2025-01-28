using UnityEngine;

public class ItemDropOff : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out SurvivorController survivorController))
        {
            Item item = survivorController.Hotbar.GetActiveItem();
            if (item != null)
            {
                survivorController.Hotbar.DropItem();
                item.transform.position = transform.position; 
            }
        }
    }
}
