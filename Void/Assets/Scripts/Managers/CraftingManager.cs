using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : Singleton<CraftingManager>
{
    [SerializeField] private List<RecipeData> recipes;
    private Item testItem;

    public Item CraftItem(RecipeData recipe)
    {
        ItemData itemData = recipe.Item;
        GameObject spawnedItem = Instantiate(itemData.ItemPrefab, Vector3.up, Quaternion.identity);
        spawnedItem.TryGetComponent(out Item item);
        return item;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            testItem = CraftItem(recipes[0]);
        }
        else if (Input.GetKeyDown(KeyCode.F) && testItem != null)
        {
            testItem.Use();
        }
    }
}