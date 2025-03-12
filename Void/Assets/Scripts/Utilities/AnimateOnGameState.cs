using System;
using UnityEngine;

public class AnimateOnGameState : MonoBehaviour
{
    [Serializable]
    private class AnimateOnGameStateData
    {
        public string animatorBoolName;
        public bool value;
        public GameManager.GameState gameState;
    }

    [SerializeField] private AnimateOnGameStateData[] animateOnGameStateData;
    private Animator animator;

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnGameStateChanged(GameManager.GameState gameState)
    {
        foreach (var data in animateOnGameStateData)
        {
            if (data.gameState == gameState)
            {
                animator.SetBool(data.animatorBoolName, data.value);
            }
        }
    }
}