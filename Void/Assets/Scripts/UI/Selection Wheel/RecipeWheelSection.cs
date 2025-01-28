using UnityEngine;

public class RecipeWheelSection : WheelSection
{
    private RecipeWheelSectionData recipeWheelSectionData;
    private Inventory inventory;

    public override void Activate()
    {
        if (!CanActivate()) return;

        CraftingManager.Instance.CraftItem(recipeWheelSectionData.RecipeData, inventory);
    }

    public override bool CanActivate()
    {
        if (!base.CanActivate()) return false;

        if (!inventory || !recipeWheelSectionData) return false;

        return CraftingManager.CanCraftItem(recipeWheelSectionData.RecipeData, inventory);
    }

    public override void InitializeData(WheelSectionData data)
    {
        base.InitializeData(data);
        recipeWheelSectionData = data as RecipeWheelSectionData;
    }

    public override void OnSetupUI(GameObject player)
    {
        if (player.TryGetComponent(out Inventory inventory))
        {
            this.inventory = inventory;
        }
    }
}
