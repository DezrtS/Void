using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EdgeComparer : IComparer<(float weight, Edge edge)>
{
    public int Compare((float weight, Edge edge) x, (float weight, Edge edge) y)
    {
        int weightComparison = x.weight.CompareTo(y.weight);
        if (weightComparison != 0)
        {
            return weightComparison;
        }

        // If weights are equal, compare edges by their hash codes or other criteria
        int aComparison = x.edge.A.GetHashCode().CompareTo(y.edge.A.GetHashCode());
        if (aComparison != 0)
        {
            return aComparison;
        }

        return x.edge.B.GetHashCode().CompareTo(y.edge.B.GetHashCode());
    }
}

public class PrimMST
{
    public List<Edge> MinimumSpanningTree;
    public List<Edge> DiscardedEdges;

    public PrimMST()
    {
        MinimumSpanningTree = new List<Edge>();
    }

    public static PrimMST Construct(List<Edge> edges, Vector2 start)
    {
        PrimMST primMST = new PrimMST();
        primMST.DiscardedEdges = new List<Edge> (edges);
        Dictionary<Vector2, List<Edge>> graph = ConstructGraph(edges);
        primMST.Construct(graph, start);
        return primMST;
    }

    private static Dictionary<Vector2, List<Edge>> ConstructGraph(List<Edge> edges)
    {
        Dictionary<Vector2, List<Edge>> graph = new Dictionary<Vector2, List<Edge>>();

        foreach (Edge edge in edges)
        {
            if (!graph.ContainsKey(edge.A))
            {
                graph[edge.A] = new List<Edge>();
            }
            if (!graph.ContainsKey(edge.B))
            {
                graph[edge.B] = new List<Edge>();
            }
            graph[edge.A].Add(new Edge(edge.A, edge.B));
            graph[edge.B].Add(new Edge(edge.B, edge.A));
        }

        return graph;
    }

    private void Construct(Dictionary<Vector2, List<Edge>> graph, Vector2 start)
    {
        var visited = new HashSet<Vector2>();
        var priorityQueue = new SortedSet<(float weight, Edge edge)>(new EdgeComparer());

        void AddEdges(Vector2 node)
        {
            visited.Add(node);
            foreach (var edge in graph[node])
            {
                Vector2 otherNode = edge.B;
                if (!visited.Contains(otherNode))
                {
                    float edgeLength = edge.Length();
                    priorityQueue.Add((edgeLength, edge));
                }
            }
        }

        AddEdges(start);

        while (priorityQueue.Count > 0)
        {
            var minEdge = priorityQueue.Min;
            priorityQueue.Remove(minEdge);

            Vector2 nextNode = minEdge.edge.B;

            if (visited.Contains(nextNode))
            {
                continue;
            }

            MinimumSpanningTree.Add(minEdge.edge);
            DiscardedEdges.Remove(minEdge.edge);
            AddEdges(nextNode);
        }
    }
}
