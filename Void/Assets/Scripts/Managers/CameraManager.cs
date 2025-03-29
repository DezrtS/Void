using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    private CinemachineCamera cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        cinemachineBasicMultiChannelPerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public static void TriggerScreenShake(float duration, float amplitude, float frequency)
    {
        if (Instance != null)
        {
            Instance.HandleScreenShake(duration, amplitude, frequency);
        }
    }

    private void HandleScreenShake(float duration, float amplitude, float frequency)
    {
        StopAllCoroutines();
        cinemachineBasicMultiChannelPerlin.AmplitudeGain = amplitude;
        cinemachineBasicMultiChannelPerlin.FrequencyGain = frequency;
        StartCoroutine(ResetScreenShakeCoroutine(duration));
    }

    private IEnumerator ResetScreenShakeCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0;
        cinemachineBasicMultiChannelPerlin.FrequencyGain = 0;
    }
}
