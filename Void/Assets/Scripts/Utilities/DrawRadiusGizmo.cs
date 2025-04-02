using UnityEngine;

[ExecuteInEditMode]
public class DrawRadiusGizmo : MonoBehaviour
{
    [Header("Gizmo Settings")]
    [Tooltip("The radius to draw around the object")]
    public float radius = 5f;

    [Tooltip("Color of the radius circle")]
    public Color gizmoColor = Color.cyan;

    [Tooltip("Should the radius be drawn as a wireframe or solid")]
    public bool wireframe = true;

    [Tooltip("Number of segments in the circle (more = smoother)")]
    [Range(3, 128)]
    public int segments = 32;

    [Tooltip("Offset from the object's position")]
    public Vector3 offset = Vector3.zero;

    void OnDrawGizmos()
    {
        // Set the gizmo color
        Gizmos.color = gizmoColor;

        // Calculate the position with offset
        Vector3 position = transform.position + offset;

        if (wireframe)
        {
            // Draw wire sphere if wireframe is selected
            Gizmos.DrawWireSphere(position, radius);
        }
        else
        {
            // Draw solid sphere if wireframe is not selected
            Gizmos.DrawSphere(position, radius);
        }

        // Additional flat circle for better visibility
        DrawFlatCircle(position, radius, segments, gizmoColor);
    }

    // Helper method to draw a flat circle at the object's position
    private void DrawFlatCircle(Vector3 position, float radius, int segments, Color color)
    {
        // Save the original color
        Color originalColor = Gizmos.color;
        Gizmos.color = color;

        // Calculate segment angle
        float angle = 0f;
        float angleIncrement = (2f * Mathf.PI) / segments;

        Vector3 prevPoint = position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

        for (int i = 0; i <= segments; i++)
        {
            angle += angleIncrement;
            Vector3 nextPoint = position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        // Restore the original color
        Gizmos.color = originalColor;
    }
}