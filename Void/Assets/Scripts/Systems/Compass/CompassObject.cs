using UnityEngine;

public class CompassObject : MonoBehaviour
{
    [SerializeField] private GameObject compassIconPrefab;
    private CompassIcon compassIcon;

    private void Awake()
    {
        compassIcon = UIManager.Instance.AddCompassIcon(compassIconPrefab);
        compassIcon.SetTarget(CameraManager.Instance.transform, transform);
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
