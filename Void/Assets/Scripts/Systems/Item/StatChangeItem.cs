using UnityEngine;

public class StatChangeItem : Item
{
    [SerializeField] private StatChangesData statChangesData;

    public override void Use()
    {
        base.Use();
        Activate();
    }

    public void Activate()
    {
        if (!NetworkItem.IsServer) return;

        PlayerStats playerStats = GetComponentInParent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.Log("Player Stats == NULL");
            return;
        }

        playerStats.RequestChangeStats(statChangesData);
    }
}