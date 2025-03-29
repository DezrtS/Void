using UnityEngine;

public class CompassObject : MonoBehaviour
{
    [SerializeField] private GameObject compassIconPrefab;
    [SerializeField] private bool disableCompassIconOnInitialize;
    private CompassIcon compassIcon;

    private void Awake()
    {
        if (CameraManager.Instance)
        {
            InitializeCompassIcon(CameraManager.Instance);
        }
        else
        {
            CameraManager.OnSingletonInitialized += InitializeCompassIcon;
        }
    }

    public void InitializeCompassIcon(CameraManager instance)
    {
        CameraManager.OnSingletonInitialized -= InitializeCompassIcon;
        compassIcon = UIManager.Instance.AddCompassIcon(compassIconPrefab);
        compassIcon.SetTarget(CameraManager.Instance.transform, transform);
        if (disableCompassIconOnInitialize) DisableCompassIcon();
    }

    public void EnableCompassIcon()
    {
        compassIcon.gameObject.SetActive(true);
    }

    public void DisableCompassIcon()
    {
        compassIcon.gameObject.SetActive(false);
    }
}
