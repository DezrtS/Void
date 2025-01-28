using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemManager : NetworkSingleton<ItemManager>
{

    private void Start()
    {
        string logFilePath = System.IO.Path.Combine(Application.persistentDataPath, "AnalyticsLog.csv");
        EventLoggerInterop.InitializeLogger(logFilePath);
        Debug.Log("Log File Path: " + logFilePath);
    }

    public void CreateItemPickUpLog(ulong clientId, Item item)
    {
        EventLoggerInterop.CreateEventLog("ItemEvent:PickUp", new string[] { "ClientId", "ItemName", "ItemDescription" }, new string[] { clientId.ToString(), item.ItemData.Name, item.ItemData.Description }, 3);
    }

    public void CreateItemDropLog(ulong clientId, Item item)
    {
        EventLoggerInterop.CreateEventLog("ItemEvent:Drop", new string[] { "ClientId", "ItemName", "ItemDescription" }, new string[] { clientId.ToString(), item.ItemData.Name, item.ItemData.Description }, 3);
    }

    private void OnDisable()
    {
        EventLoggerInterop.ShutdownLogger();
    }

    public static ulong SpawnNetworkedItem(ItemData itemData)
    {
        Item item = SpawnItem(itemData);
        return item.NetworkObjectId;
    }

    public static Item SpawnItem(ItemData itemData)
    {
        GameObject spawnedItem = Instantiate(itemData.ItemPrefab);
        Item item = spawnedItem.GetComponent<Item>();
        item.NetworkObject.Spawn();
        return item;
    }
}