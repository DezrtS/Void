using UnityEngine;

public class DisableOnKey : MonoBehaviour
{
    [SerializeField] private KeyCode key = KeyCode.H;
    [SerializeField] private GameObject disableEnable;
    private bool isEnabled = true;

    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            isEnabled = !isEnabled;
            disableEnable.SetActive(isEnabled);
        }
    }
}
