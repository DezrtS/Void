using UnityEngine;

[CreateAssetMenu(fileName = "RecipeWheelSectionData", menuName = "ScriptableObjects/SelectionWheel/RecipeWheelSectionData")]
public class RecipeWheelSectionData : WheelSectionData
{
    [SerializeField] private bool overrideSectionTitle;
    [SerializeField] private float craftingCooldown;
    [SerializeField] private RecipeData recipeData;

    public float CraftingCooldown => craftingCooldown;
    public RecipeData RecipeData => recipeData;

    public override string GetTitle()
    {
        if (overrideSectionTitle) return recipeData.Item.Name;

        return base.GetTitle();
    }

    public override string GetSecondaryText()
    {
        string recipe = "";

        foreach (ResourceRequirement resourceRequirement in recipeData.ResourceRequirements)
        {
            recipe += $"{resourceRequirement.Amount} {resourceRequirement.Resource.Name}\n";
        }

        return recipe;
    }
}