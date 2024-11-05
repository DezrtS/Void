using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int hotbarCapacity;
    [SerializeField] private Transform activeTransform;
    [SerializeField] private Transform[] storageTransforms;
    private int selectedIndex;
    private Item[] hotbar;
    private Dictionary<ResourceData, int> inventory;

    public delegate void ItemEventHandler(int index, Item item);
    public event ItemEventHandler OnPickUpItem;
    public event ItemEventHandler OnDropItem;
    public event ItemEventHandler OnSwitchItem;

    public delegate void ResourceEventHandler(ResourceData resource, int amount);
    public event ResourceEventHandler OnResourceEvent;

    public void Awake()
    {
        hotbar = new Item[hotbarCapacity];
        inventory = new Dictionary<ResourceData, int>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Drop();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (!SelectedItem())
            {
                if (CraftingManager.Instance.GetTestItem())
                {
                    PickUp(CraftingManager.Instance.GetTestItem());
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.F) && SelectedItem())
        {
            SelectedItem().Use();
        }
        else if (Input.GetKeyUp(KeyCode.F) && SelectedItem())
        {
            SelectedItem().StopUsing();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchItem(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchItem(false);
        }
    }

    public void PickUp(Item item)
    {
        item.PickUp();
        item.transform.parent = activeTransform;
        item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        hotbar[selectedIndex] = item;
    }

    public void Drop()
    {
        Item item = SelectedItem();
        if (item)
        {
            item.Drop();
            item.transform.parent = null;
            hotbar[selectedIndex] = null;
        }
    }

    public Item SelectedItem()
    {
        return hotbar[selectedIndex];
    }

    public Item SwitchItem(bool left)
    {
        int newIndex = selectedIndex;
        newIndex += left ? -1 : 1;

        if (newIndex < 0)
        {
            newIndex = hotbarCapacity - 1;
        }
        else if (newIndex >= hotbarCapacity)
        {
            newIndex = 0;
        }

        SwapAndStore(selectedIndex, newIndex);
        selectedIndex = newIndex;

        return hotbar[selectedIndex];
    }

    public Item SwitchItem(int index)
    {
        if (index >= 0 && index < hotbarCapacity && index != selectedIndex)
        {
            SwapAndStore(selectedIndex, index);
            selectedIndex = index;
            return hotbar[selectedIndex];
        }
        return null;
    }

    public void SwapAndStore(int fromIndex, int toIndex)
    {
        Item from = hotbar[fromIndex];
        Item to = hotbar[toIndex];

        if (from)
        {
            if (storageTransforms.Length >= fromIndex)
            {
                from.transform.parent = storageTransforms[fromIndex];
                from.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            else
            {
                from.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        if (to)
        {
            if (storageTransforms.Length >= toIndex)
            {
                to.transform.parent = activeTransform;
                to.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            else
            {
                to.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    public void AddResource(ResourceData resource, int amount)
    {
        if (inventory.ContainsKey(resource))
        {
            inventory[resource] = inventory[resource] + amount;
        }
        else
        {
            inventory.Add(resource, amount);
        }
    }
}