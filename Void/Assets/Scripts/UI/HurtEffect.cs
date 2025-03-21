using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class HurtEffect : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private float maxVignetteIntensity = 0.6f;
    private Health health;
    private Vignette vignette;

    private void OnEnable()
    {
        UIManager.OnSetupUI += OnSetupUI;
        InitializeVignette();
    }

    private void OnDisable()
    {
        UIManager.OnSetupUI -= OnSetupUI;

        if (health)
        {
            health.OnCurrentHealthChanged -= OnHealthChange;
        }
    }

    private void InitializeVignette()
    {
        if (volume != null && volume.profile != null)
        {
            if (!volume.profile.TryGet(out vignette))
            {
                Debug.LogWarning("Vignette effect not found in volume profile");
            }
        }
    }

    public void OnSetupUI(GameObject player)
    {
        if (player.TryGetComponent(out Health health))
        {
            Attach(health);
        }
    }

    public void Attach(Health health)
    {
        this.health = health;
        health.OnCurrentHealthChanged += OnHealthChange;
    }

    public void Detach(Health health)
    {
        health.OnCurrentHealthChanged -= OnHealthChange;
        this.health = null;
    }

    public void OnHealthChange(float previousValue, float newValue, float maxValue)
    {
        if (vignette == null) return;

        float healthPercentage = newValue / maxValue;

        // Inverse the percentage so lower health = stronger vignette
        float vignetteStrength = (1 - healthPercentage) * maxVignetteIntensity;

        vignette.intensity.Override(vignetteStrength);
    }
}