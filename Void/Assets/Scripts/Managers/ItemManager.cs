using UnityEngine;

public class ItemManager : Singleton<ItemManager>
{

    private void Start()
    {
        string logFilePath = System.IO.Path.Combine(Application.persistentDataPath, "AnalyticsLog.csv");
        EventLoggerInterop.InitializeLogger(logFilePath);
        Debug.Log("Log File Path: " + logFilePath);
    }

    public static void CreateSimpleEventLog(string eventName, string eventValue)
    {
        EventLoggerInterop.CreateEventLog("SimpleEvent:Event", new string[] { "EventName", "EventValue" }, new string[] { eventName, eventValue }, 2);
    }

    public static void CreateItemPickUpLog(ulong clientId, Item item)
    {
        EventLoggerInterop.CreateEventLog("ItemEvent:PickUp", new string[] { "ClientId", "ItemName", "ItemDescription" }, new string[] { clientId.ToString(), item.ItemData.Name, item.ItemData.Description }, 3);
    }

    public static void CreateItemDropLog(ulong clientId, Item item)
    {
        EventLoggerInterop.CreateEventLog("ItemEvent:Drop", new string[] { "ClientId", "ItemName", "ItemDescription" }, new string[] { clientId.ToString(), item.ItemData.Name, item.ItemData.Description }, 3);
    }

    private void OnDisable()
    {
        EventLoggerInterop.ShutdownLogger();
    }
}