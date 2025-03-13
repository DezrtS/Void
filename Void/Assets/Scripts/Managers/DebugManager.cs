
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : Singleton<DebugManager>
{
    public bool enablePathVisualization;
    public int index = 0;
    public float tileSize;
    public float scale = 70;
    public List<(Grid2D<MultiNodePathfinder2D.VisualNode>, CommandInvoker)> pathDebugging = new List<(Grid2D<MultiNodePathfinder2D.VisualNode>, CommandInvoker)>();

    private void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            if (!enablePathVisualization || pathDebugging.Count <= 0) return;

            pathDebugging[index].Item2.Undo();
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (!enablePathVisualization || pathDebugging.Count <= 0) return;

            pathDebugging[index].Item2.Redo();
        }
    }

    private void OnDrawGizmos()
    {
        if (!enablePathVisualization || pathDebugging.Count <= 0) return;

        (Grid2D<MultiNodePathfinder2D.VisualNode> grid, CommandInvoker invoker) path = pathDebugging[index];

        for (int x = 0; x < path.grid.Size.x; x++)
        {
            for (int y = 0; y < path.grid.Size.y; y++)
            {
                Gizmos.color = path.grid[x, y].Color;
                Gizmos.DrawSphere(new Vector3(x * tileSize, 0, y * tileSize), (path.grid[x, y].Cost + 10) / scale);
            }
        }
    }
}

public static class DebugDraw
{
    // Draw the BoxCast volume and direction
    public static void DrawBoxCast(Vector3 origin, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float distance, Color color, float duration = 0.1f)
    {
        // Draw the starting box
        DrawBox(origin, halfExtents, orientation, color, duration);

        // Draw the ending position of the cast
        Vector3 endPosition = origin + direction.normalized * distance;
        DrawBox(endPosition, halfExtents, orientation, color, duration);

        // Draw lines connecting the corners of the start and end boxes
        Vector3[] startCorners = GetBoxCorners(origin, halfExtents, orientation);
        Vector3[] endCorners = GetBoxCorners(endPosition, halfExtents, orientation);

        for (int i = 0; i < startCorners.Length; i++)
        {
            Debug.DrawLine(startCorners[i], endCorners[i], color, duration);
        }
    }

    // Helper to get all 8 corners of a box
    private static Vector3[] GetBoxCorners(Vector3 center, Vector3 halfExtents, Quaternion orientation)
    {
        Vector3[] corners = new Vector3[8];
        int index = 0;

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 localOffset = new Vector3(
                        x * halfExtents.x,
                        y * halfExtents.y,
                        z * halfExtents.z
                    );
                    corners[index++] = center + orientation * localOffset;
                }
            }
        }
        return corners;
    }

    // Helper to draw a box wireframe
    private static void DrawBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, Color color, float duration)
    {
        Vector3[] corners = GetBoxCorners(center, halfExtents, orientation);

        // Define edges (12 lines per box)
        int[] edges = {
            0,1, 1,3, 3,2, 2,0, // Front face
            4,5, 5,7, 7,6, 6,4, // Back face
            0,4, 1,5, 2,6, 3,7  // Connecting front/back
        };

        for (int i = 0; i < edges.Length; i += 2)
        {
            Debug.DrawLine(
                corners[edges[i]],
                corners[edges[i + 1]],
                color,
                duration
            );
        }
    }
}