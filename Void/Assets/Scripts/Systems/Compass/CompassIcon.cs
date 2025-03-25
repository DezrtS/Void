using UnityEngine;

public class CompassIcon : MonoBehaviour
{
    private bool isActive;
    private Transform compassObjectTransform;
    private Transform targetTransform;
    private RectTransform compassRect;
    private float maxX;

    private void OnEnable()
    {
        UpdatePosition();
    }

    private void Start()
    {
        // Cache reference to parent compass RectTransform
        compassRect = transform.parent.GetComponent<RectTransform>();
        if (compassRect != null)
        {
            maxX = compassRect.rect.width / 2f;
        }
        else
        {
            Debug.LogError("CompassIcon requires a RectTransform parent!");
        }
    }

    private void FixedUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (isActive && compassRect != null)
        {
            // Calculate direction to target (XZ plane only)
            Vector3 direction = compassObjectTransform.position - targetTransform.position;
            direction.y = 0;

            // Handle case where target is directly above/below
            if (direction.magnitude < 0.01f)
            {
                transform.localPosition = Vector3.zero;
                return;
            }

            direction.Normalize();

            // Get player's forward direction (XZ plane)
            Vector3 playerForward = targetTransform.forward;
            playerForward.y = 0;
            playerForward.Normalize();

            // Calculate signed angle between directions
            float angle = Vector3.SignedAngle(playerForward, direction, Vector3.up);

            // Convert angle to screen position
            float localX = Mathf.Clamp(angle / 180f * maxX, -maxX, maxX);
            transform.localPosition = new Vector3(localX, 0f, 0f);
        }
    }

    public void SetTarget(Transform player, Transform target)
    {
        targetTransform = player;
        compassObjectTransform = target;
        isActive = true;
    }
}