using UnityEngine;

public class DamageListener : MonoBehaviour
{
    public void HandleDamageTaken(int currentHealth, int totalHealth)
    {
        Debug.Log($"Damage Taken! Current Health: {currentHealth}/{totalHealth}");
    }

    public void HandleDeath()
    {
        Debug.Log("The object has died!");
    }
}