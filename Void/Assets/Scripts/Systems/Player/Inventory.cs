using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    private Dictionary<ResourceData, int> resources;

    public delegate void ResourceEventHandler(ResourceData resource, int amount);
    public event ResourceEventHandler OnResourceEvent;

    public Dictionary<ResourceData, int> Resources => resources;

    public void Awake()
    {
        resources = new Dictionary<ResourceData, int>();
    }

    public void AddResource(ResourceData resource, int amount)
    {
        AddResourceClientServerSide(resource, amount);
        AddResourceServerRpc(GameDataManager.Instance.GetResourceDataIndex(resource), amount);
    }

    private void AddResourceClientServerSide(ResourceData resource, int amount)
    {
        if (resources.ContainsKey(resource))
        {
            resources[resource] = resources[resource] + amount;
        }
        else
        {
            resources.Add(resource, amount);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddResourceServerRpc(int resourceIndex, int amount)
    {
        AddResourceClientServerSide(GameDataManager.Instance.GetResourceData(resourceIndex), amount);
    }

    public void RemoveResource(ResourceData resource, int amount)
    {
        RemoveResourceClientServerSide(resource, amount);
        RemoveResourceServerRpc(GameDataManager.Instance.GetResourceDataIndex(resource), amount);
    }

    private void RemoveResourceClientServerSide(ResourceData resource, int amount)
    {
        if (resources.ContainsKey(resource))
        {
            resources[resource] = resources[resource] - amount;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveResourceServerRpc(int resourceIndex, int amount)
    {
        AddResourceClientServerSide(GameDataManager.Instance.GetResourceData(resourceIndex), amount);
    }
}