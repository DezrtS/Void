using UnityEngine;

public interface IInteractable
{
    public abstract void Interact(GameObject interactor);
    public abstract InteractableData GetInteractableData();
}