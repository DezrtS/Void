using UnityEngine;
using UnityEngine.UI;

public class RecipeWheelSection : WheelSection
{
    [SerializeField] private Image mainIcon;
    private RecipeWheelSectionData recipeWheelSectionData;
    private Inventory inventory;

    private float cooldownTimer;

    public override void UpdateTimers(float deltaTime)
    {
        base.UpdateTimers(deltaTime);
        if (cooldownTimer > 0)
        {
            cooldownTimer -= deltaTime;
            if (cooldownTimer <= 0)
            {
                cooldownTimer = 0;
                mainIcon.fillAmount = 1;
            }
            else
            {
                mainIcon.fillAmount = 1 - (cooldownTimer / recipeWheelSectionData.CraftingCooldown);
            }
        }
    }

    public override void Activate()
    {
        if (!CanActivate()) return;

        cooldownTimer = recipeWheelSectionData.CraftingCooldown;
        mainIcon.fillAmount = 0;
        CraftingManager.Instance.CraftItem(recipeWheelSectionData.RecipeData, inventory);
    }

    public override bool CanActivate()
    {
        if (!base.CanActivate()) return false;

        if (!inventory || !recipeWheelSectionData || cooldownTimer > 0) return false;

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
