using UnityEngine;
using TMPro;
using Unity.Netcode;

public class NetworkTimer : NetworkBehaviour
{
    [SerializeField] private TMP_Text timerText; 
    [SerializeField] private float maxTime = 300f; 


    private NetworkVariable<float> timer = new NetworkVariable<float>(
        300f, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool isRunning = true;

    private void Update()
    {
        if (IsServer && isRunning)
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

    public void StartTimer()
    {
        if (IsServer)
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
}

