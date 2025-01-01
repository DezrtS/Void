
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
