using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeVisualizer : MonoBehaviour
{
    public Material lineMaterial;
    private GameObject lineHolder;

    private void Awake()
    {
        lineHolder = new GameObject("Line Holder");
    }

    public void DrawEdges(List<Prim.Edge> edges, Vector3 offset, float scale, string name, bool swapYZ)
    {
        GameObject lineCollectionHolder = new GameObject(name);
        lineCollectionHolder.transform.SetParent(lineHolder.transform);

        foreach (var edge in edges)
        {
            Vector3 position1 = edge.U.Position;
            Vector3 position2 = edge.V.Position;

            if (swapYZ)
            {
                position1 = new Vector3(position1.x, position1.z, position1.y);
                position2 = new Vector3(position2.x, position2.z, position2.y);
            }

            GameObject line = DrawLine((position1 + offset) * scale, (position2 + offset) * scale);
            line.transform.SetParent(lineCollectionHolder.transform);
        }
    }

    GameObject DrawLine(Vector3 p1, Vector3 p2)
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
        return line;
    }

    public void ClearLines()
    {
        Destroy(lineHolder);
        lineHolder = new GameObject("Line Holder");
    }
}
