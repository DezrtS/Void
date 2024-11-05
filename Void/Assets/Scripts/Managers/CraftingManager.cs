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

    public Item CraftItem(RecipeData recipe, Dictionary<ResourceData, int> inventory)
    {
        foreach (ResourceRequirement resourceRequirement in recipe.ResourceRequirements)
        {
            if (inventory.TryGetValue(resourceRequirement.Resource, out int amount))
            {
                if (amount >= resourceRequirement.Amount)
                {
                    int newAmount = amount - resourceRequirement.Amount;
                    if (newAmount <= 0)
                    {
                        inventory.Remove(resourceRequirement.Resource);
                    }
                    else
                    {
                        inventory[resourceRequirement.Resource] = amount - resourceRequirement.Amount;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        return CraftItem(recipe);
    }

    public Item GetTestItem()
    {
        return testItem;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            testItem = CraftItem(recipes[0]);
        }
    }
}