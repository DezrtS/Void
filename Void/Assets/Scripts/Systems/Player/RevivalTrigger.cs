using UnityEngine;

public class RevivalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Health health))
        {
            if (health.NetworkHealth.IsServer && health.IsDead)
            {
                health.RequestRespawn();
            }
        }
    }
}