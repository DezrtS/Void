using System;
using Unity.Netcode;
using UnityEngine;

public class ItemDropOff : MonoBehaviour, IInteractable
{
    public delegate void ItemDropOffHandler(Item item, ItemDropOff itemDropOff);
    public static event ItemDropOffHandler OnDropOff;

    [SerializeField] private float processingTime;
    [SerializeField] private float ejectionPower;
    private float processingTimer;
    private Item item;

    public void Interact(GameObject interactor)
    {
        if (item != null) return;

        if (interactor.TryGetComponent(out SurvivorController survivorController))
        {
            Item item = survivorController.Hotbar.GetItem();
            if (item != null)
            {
                if (!item.CanDrop()) return;
                survivorController.Hotbar.DropItem();
                ProcessItemServerRpc(item.NetworkObjectId);
            }
        }
    }

    private void FixedUpdate()
    {
        if (processingTimer > 0)
        {
            processingTimer -= Time.fixedDeltaTime;
            if (processingTimer <= 0)
            {
                processingTimer = 0;
                EjectItem();
            }
        }
    }

    public void ProcessItemClientServerSide(Item item)
    {
        this.item = item;
        item.transform.position = transform.position;
        item.PickUp();
        OnDropOff?.Invoke(item, this);
        processingTimer = processingTime;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ProcessItemServerRpc(ulong networkObjectId)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        Item item = networkObject.GetComponent<Item>();
        ProcessItemClientServerSide(item);
    }

    [ClientRpc(RequireOwnership = false)]
    public void ProcessItemClientRpc(ulong networkObjectId, ClientRpcParams clientRpcParams = default)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        Item item = networkObject.GetComponent<Item>();
        ProcessItemClientServerSide(item);
    }

    public void AcceptItem()
    {
        if (item == null) return;

        Debug.Log("Item Accepted");
        item = null;
    }

    public void EjectItem()
    {
        if (item == null) return;
        item.Drop();
        Rigidbody rig = item.GetComponent<Rigidbody>();
        rig.AddForce(transform.rotation * new Vector3(0, 1, 1) * ejectionPower, ForceMode.Impulse);
        item = null;
    }
}
