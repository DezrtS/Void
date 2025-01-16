using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : Singleton<GameDataManager>
{
    [SerializeField] private List<ItemData> items = new List<ItemData>();
    [SerializeField] private List<RecipeData> recipes = new List<RecipeData>();
    [SerializeField] private List<ResourceData> resources = new List<ResourceData>();

    public int GetItemDataIndex(ItemData itemData)
    {
        return items.IndexOf(itemData);
    }

    public ItemData GetItemData(int index)
    {
        return items[index];
    }

    public int GetRecipeDataIndex(RecipeData recipeData)
    {
        return recipes.IndexOf(recipeData);
    }

    public RecipeData GetRecipeData(int index)
    {
        return recipes[index];
    }

    public int GetResourceDataIndex(ResourceData resourceData)
    {
        return resources.IndexOf(resourceData);
    }

    public ResourceData GetResourceData(int index)
    {
        return resources[index];
    }
}
