using Unity.Netcode;
using UnityEngine;

public class CraftingManager : NetworkSingleton<CraftingManager>
{
    public void TryCraftItem(RecipeData recipeData, Inventory inventory)
    {
        if (CanCraftItem(recipeData, inventory)) CraftItem(recipeData, inventory);
    }

    public void CraftItem(RecipeData recipeData, Inventory inventory)
    {
        foreach (ResourceRequirement resourceRequirement in recipeData.ResourceRequirements)
        {
            if (resourceRequirement.Amount <= 0) continue;
            inventory.RemoveResource(resourceRequirement.Resource, resourceRequirement.Amount);
        }

        CraftItemServerRpc(GameDataManager.Instance.GetRecipeDataIndex(recipeData), inventory.NetworkObjectId);
    }

    public static bool CanCraftItem(RecipeData recipeData, Inventory inventory)
    {
        foreach (ResourceRequirement resourceRequirement in recipeData.ResourceRequirements)
        {
            if (resourceRequirement.Amount <= 0) continue;

            if (inventory.Resources.TryGetValue(resourceRequirement.Resource, out int amount))
            {
                if (amount < resourceRequirement.Amount) return false;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void CraftItemServerRpc(int recipeIndex, ulong networkObjectId, ServerRpcParams rpcParams = default)
    {
        RecipeData recipeData = GameDataManager.Instance.GetRecipeData(recipeIndex);
        NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        Inventory inventory = networkObject.GetComponent<Inventory>();
        Hotbar hotbar = networkObject.GetComponent<Hotbar>();

        if (CanCraftItem(recipeData, inventory))
        {
            hotbar.RequestPickUpItem(recipeData.Item);
        }
    }
}