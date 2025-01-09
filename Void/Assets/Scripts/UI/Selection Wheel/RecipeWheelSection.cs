using UnityEngine;

public class RecipeWheelSection : WheelSection
{
    public override void Activate()
    {
        Debug.Log("Wheel Section Activated");
    }

    public override bool CanActivate()
    {
        return true;
    }
}
