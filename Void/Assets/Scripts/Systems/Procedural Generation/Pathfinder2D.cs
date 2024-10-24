using BlueRaja;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder2D
{
    public class Node
    {
        public Vector2Int Position { get; private set; }
        public Node Previous { get; set; }
        public float Cost { get; set; }

        public Node(Vector2Int position)
        {
            Position = position;
        }
    }

    public struct PathInfo
    {
        public bool traversable;
        public float cost;
        public bool isGoal;
        public bool isStart;
    }

    static readonly Vector2Int[] neighbors = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    Grid2D<Node> grid;
    SimplePriorityQueue<Node, float> queue;
    HashSet<Node> closed;
    Stack<Vector2Int> stack;
    private float straightness = 0.5f;

    public Pathfinder2D(Vector2Int size)
    {
        grid = new Grid2D<Node>(size, Vector2Int.zero);

        queue = new SimplePriorityQueue<Node, float>();
        closed = new HashSet<Node>();
        stack = new Stack<Vector2Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                grid[x, y] = new Node(new Vector2Int(x, y));
            }
        }
    }

    void ResetNodes()
    {
        var size = grid.Size;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var node = grid[x, y];
                node.Previous = null;
                node.Cost = float.PositiveInfinity;
            }
        }
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, Func<Node, Node, PathInfo> costFunction)
    {
        ResetNodes();
        queue.Clear();
        closed.Clear();

        queue = new SimplePriorityQueue<Node, float>();
        closed = new HashSet<Node>();

        grid[start].Cost = 0;
        queue.Enqueue(grid[start], 0);
        bool wasHorizontal = end.x -  start.x > end.y - start.y;
        int horizontalCostFactor = 1;

        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            closed.Add(node);

            if (node.Previous != null)
            {
                bool isHorizontal = node.Position.y == node.Previous.Position.y;

                if (isHorizontal != wasHorizontal)
                {
                    wasHorizontal = isHorizontal;
                    horizontalCostFactor++;
                }
            }

            if (node.Position == end)
            {
                return ReconstructPath(node);
            }

            foreach (var offset in neighbors)
            {
                if (!grid.InBounds(node.Position + offset)) continue;
                var neighbor = grid[node.Position + offset];
                if (closed.Contains(neighbor)) continue;

                bool isHorizontal = node.Position.y == neighbor.Position.y;
                var pathCost = costFunction(node, neighbor);
                if (isHorizontal != wasHorizontal)
                {
                    pathCost.cost += straightness * horizontalCostFactor;
                }
                float newCost;
                if (pathCost.isGoal) 
                {
                    neighbor.Previous = node;
                    return ReconstructPath(neighbor);    
                }
                else if (pathCost.isStart)
                {
                    newCost = node.Cost + pathCost.cost;

                    if (newCost < neighbor.Cost)
                    {
                        neighbor.Cost = newCost;

                        if (queue.TryGetPriority(node, out float existingPriority))
                        {
                            queue.UpdatePriority(node, newCost);
                        }
                        else
                        {
                            queue.Enqueue(neighbor, neighbor.Cost);
                        }
                    }

                    continue;
                }
                if (!pathCost.traversable) continue;

                newCost = node.Cost + pathCost.cost;

                if (newCost < neighbor.Cost)
                {
                    neighbor.Previous = node;
                    neighbor.Cost = newCost;

                    if (queue.TryGetPriority(node, out float existingPriority))
                    {
                        queue.UpdatePriority(node, newCost);
                    }
                    else
                    {
                        queue.Enqueue(neighbor, neighbor.Cost);
                    }
                }
            }
        }

        return null;
    }

    List<Vector2Int> ReconstructPath(Node node)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        while (node != null)
        {
            stack.Push(node.Position);
            node = node.Previous;
        }

        while (stack.Count > 0)
        {
            result.Add(stack.Pop());
        }

        return result;
    }
}