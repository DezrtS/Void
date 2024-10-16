using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeVisualizer : MonoBehaviour
{
    public Material lineMaterial;
    private GameObject lineHolder;

    public void DrawEdges(List<Edge> edges, bool useGridMap)
    {
        ClearLines();
        lineHolder = new GameObject("Line Holder");
        if (useGridMap)
        {
            foreach (var edge in edges)
            {
                DrawGridMapLine(edge.A, edge.B);
            }
        }
        else
        {
            foreach (var edge in edges)
            {
                DrawLine(edge.A, edge.B);
            }
        }
    }

    void DrawLine(Vector2 p1, Vector2 p2)
    {
        GameObject line = new GameObject("Edge");
        line.transform.parent = lineHolder.transform;
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = 0.5f;
        lr.endWidth = 0.5f;
        lr.positionCount = 2;
        lr.SetPosition(0, p1);
        lr.SetPosition(1, p2);
    }

    void DrawGridMapLine(Vector2 p1, Vector2 p2)
    {
        float tileSize = GridMapManager.Instance.MapTileSize;
        GameObject line = new GameObject("Edge");
        line.transform.parent = lineHolder.transform;
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = 0.5f;
        lr.endWidth = 0.5f;
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(p1.x * tileSize, 0.5f, p1.y * tileSize));
        lr.SetPosition(1, new Vector3(p2.x * tileSize, 0.5f, p2.y * tileSize));
    }

    public void ClearLines()
    {
        Destroy(lineHolder);
    }
}
