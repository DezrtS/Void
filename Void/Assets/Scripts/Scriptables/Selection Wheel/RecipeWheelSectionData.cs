using UnityEngine;

[CreateAssetMenu(fileName = "RecipeWheelSectionData", menuName = "Scriptable Objects/RecipeWheelSectionData")]
public class RecipeWheelSectionData : WheelSectionData
{
    [SerializeField] private bool overrideSectionTitle;
    [SerializeField] private RecipeData recipeData;
    public RecipeData RecipeData => recipeData;

    public override string GetFormattedTitle()
    {
        if (overrideSectionTitle) return recipeData.Item.Name;

        return base.GetFormattedTitle();
    }

    public override string GetFormattedDescription()
    {
        string recipe = "";

        foreach (ResourceRequirement resourceRequirement in recipeData.ResourceRequirements)
        {
            recipe += $"- {resourceRequirement.Amount} {resourceRequirement.Resource.Name}\n";
        }

        return base.GetFormattedDescription() + recipe;
    }
}