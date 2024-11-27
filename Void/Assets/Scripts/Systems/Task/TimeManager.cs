using UnityEngine;
using TMPro;
using Unity.Netcode;

public class TimeManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text timerText;  
    [SerializeField] private float maxTime = 300f; 
    [SerializeField] private GameObject generalHUD; 
    [SerializeField] private GameObject timerObject; 

    private NetworkVariable<float> timer = new NetworkVariable<float>(
        300f, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<bool> isGeneralHUDActive = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool isRunning = false;

    private void Update()
    {
        if (IsServer && isRunning && isGeneralHUDActive.Value)
        {
            if (timer.Value > 0)
            {
                timer.Value -= Time.deltaTime; 
            }
            else
            {
                timer.Value = 0; 
                isRunning = false; 
            }
        }

        UpdateTimerUI(); 
    }

    public void ActivateGeneralHUD()
    {
        if (IsServer)
        {
            isGeneralHUDActive.Value = true;

            if (generalHUD != null)
            {
                generalHUD.SetActive(true); 
            }
        }
    }

    public void StartTimer()
    {
        if (IsServer && isGeneralHUDActive.Value)
        {
            isRunning = true;
        }
    }

    public void StopTimer()
    {
        if (IsServer)
        {
            isRunning = false;
        }
    }

    public void ResetTimer()
    {
        if (IsServer)
        {
            timer.Value = maxTime;
            isRunning = false;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer.Value / 60);
            int seconds = Mathf.FloorToInt(timer.Value % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";  
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isGeneralHUDActive.OnValueChanged += OnGeneralHUDStateChanged;

        if (!IsServer && generalHUD != null)
        {
            generalHUD.SetActive(isGeneralHUDActive.Value);
        }
    }

    private void OnGeneralHUDStateChanged(bool oldValue, bool newValue)
    {
        if (generalHUD != null)
        {
            generalHUD.SetActive(newValue);
        }
    }

    private void OnDestroy()
    {
        if (isGeneralHUDActive != null)
        {
            isGeneralHUDActive.OnValueChanged -= OnGeneralHUDStateChanged;
        }
    }
}
