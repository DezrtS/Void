using UnityEngine;

public class Lights : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Panic) animator.SetTrigger("Shutdown");
    }
}
