using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private GameManager.PlayerRole playerRole;

    private void Awake()
    {
        GameManager.OnSingletonInitialized += (GameManager gameManager) =>
        {
            gameManager.AddSpawnPoint(this);
        };
    }

    public bool CanSpawn()
    {
        return true;
    }
}
