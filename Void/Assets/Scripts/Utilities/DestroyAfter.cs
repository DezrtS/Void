using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    [SerializeField] float destroyAfter;

    private void Start()
    {
        Destroy(gameObject, destroyAfter);
    }
}