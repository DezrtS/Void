using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private GameManager.PlayerRole playerRole;
    public GameManager.PlayerRole PlayerRole => playerRole;

    private void Awake()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.AddSpawnPoint(this);
        }
        else
        {
            GameManager.OnSingletonInitialized += (GameManager gameManager) =>
            {
                gameManager.AddSpawnPoint(this);
            };
        }
    }

    public bool CanSpawn()
    {
        return true;
    }
}
