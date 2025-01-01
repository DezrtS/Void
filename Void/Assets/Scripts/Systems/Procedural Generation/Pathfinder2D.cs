using BlueRaja;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Pathfinder2D;

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
            Cost = float.PositiveInfinity;
        }
    }

    public struct PathInfo
    {
        public bool traversable;
        public float cost;
    }

    private Grid2D<Node> grid;
    private SimplePriorityQueue<Node, float> queue;
    private HashSet<Node> closed;
    private Stack<Vector2Int> stack;

    public Grid2D<NodeInfo> debugGrid;
    public CommandInvoker commandInvoker;

    public Pathfinder2D(Vector2Int size)
    {
        grid = new Grid2D<Node>(size);

        queue = new SimplePriorityQueue<Node, float>();
        closed = new HashSet<Node>();
        stack = new Stack<Vector2Int>();

        debugGrid = new Grid2D<NodeInfo>(size);
        commandInvoker = new CommandInvoker();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                grid[x, y] = new Node(new Vector2Int(x, y));
                debugGrid[x, y] = new NodeInfo(Color.gray, 0);
            }
        }
    }

    // For Turning, Look at tiles ahead and behind
    // Tiles checked for turning are being added to queue for consideration
    // Cost may not be working properly
    // Final path skips tiles


    private void ResetNodes()
    {
        foreach (Node node in grid.Data)
        {
            node.Previous = null;
            node.Cost = float.PositiveInfinity;
        }

        ResetNodeGrid resetNodeGrid = new ResetNodeGrid(debugGrid);
        commandInvoker.ExecuteCommand(resetNodeGrid);
    }

    private void InitializePathfinding(Vector2Int start)
    {
        ResetNodes();
        queue.Clear();
        closed.Clear();

        grid[start].Cost = 0;
        queue.Enqueue(grid[start], 0);
    }

    public List<Vector2Int> FindPath(Vector2Int start, Func<Node, Node, PathInfo> pathFunction, Func<Node, bool> goalFunction)
    {
        InitializePathfinding(start);

        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            closed.Add(node);

            if (goalFunction(node)) return ReconstructPath(node);

            ProcessNeighbors(node, pathFunction);
        }

        return null;
    }

    public List<Vector2Int> FindPath(Vector2Int start, int additionalPathSize, Func<Node, List<Node>, PathInfo> pathFunction, Func<Node, bool> goalFunction)
    {
        InitializePathfinding(start);

        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            closed.Add(node);
            ChangeNodeColor changeNodeColor = new ChangeNodeColor(debugGrid, node.Position, Color.green);
            commandInvoker.ExecuteCommand(changeNodeColor);

            if (goalFunction(node)) return ReconstructPath(node);

            ProcessExtendedNeighbors(node, additionalPathSize, pathFunction);
        }

        return null;
    }

    private void ProcessNeighbors(Node node, Func<Node, Node, PathInfo> pathFunction)
    {
        foreach (Vector2Int offset in FacilityGeneration.DirectNeighbors)
        {
            if (!grid.InBounds(node.Position + offset)) continue;

            Node neighbor = grid[node.Position + offset];
            if (closed.Contains(neighbor)) continue;

            PathInfo pathInfo = pathFunction(node, neighbor);
            if (!pathInfo.traversable) continue;

            float newCost = node.Cost + pathInfo.cost;
            if (newCost < neighbor.Cost)
            {
                neighbor.Previous = node;
                neighbor.Cost = newCost;
                if (!queue.TryUpdatePriority(neighbor, newCost))
                {
                    queue.Enqueue(neighbor, newCost);
                }
            }
        }
    }

    private void ProcessExtendedNeighbors(Node node, int additionalPathSize, Func<Node, List<Node>, PathInfo> pathFunction)
    {
        Node previous = node.Previous;
        if (previous != null)
        {
            Vector2Int direction = node.Position - previous.Position;
            Matrix4x4 rotationMatrix = FacilityGenerationManager.GetRotationMatrix(direction);

            foreach (Vector2Int offset in FacilityGeneration.ForwardNeighbors)
            {
                // A PathSize of 0 may break this function (Current Solution may cause problems)
                Vector2Int trueOffset = (offset == Vector2Int.down) ? new Vector2Int(0, -Mathf.Abs(2 * additionalPathSize - 1)) : offset;
                if (!ProcessDirectionalNeighbors(node, additionalPathSize, rotationMatrix, trueOffset, offset, pathFunction)) return;
            }

            foreach (Vector2Int offset in FacilityGeneration.SideNeighbors)
            {
                Vector2Int trueOffset = offset; //- Vector2Int.down * Mathf.Clamp(additionalPathSize - 1, 0, additionalPathSize);
                ProcessDirectionalNeighbors(node, additionalPathSize, rotationMatrix, trueOffset, direction, pathFunction);
            }
        }
        else
        {
            foreach (Vector2Int offset in FacilityGeneration.DirectNeighbors)
            {
                ProcessDirectionalNeighbors(node, additionalPathSize, Matrix4x4.identity, offset, offset, pathFunction);
            }
        }
    }

    private bool ProcessDirectionalNeighbors(Node node, int additionalPathSize, Matrix4x4 rotationMatrix, Vector2Int offset, Vector2Int direction, Func<Node, List<Node>, PathInfo> pathFunction)
    {
        Vector2 rotatedOffset = rotationMatrix * (Vector2)offset;
        Vector2Int trueOffset = new Vector2Int((int)rotatedOffset.x, (int)rotatedOffset.y);

        Vector2 rotatedDirection = rotationMatrix * (Vector2)direction;
        Vector2Int trueDirection = new Vector2Int((int)rotatedDirection.x, (int)rotatedDirection.y);

        List<Node> neighbors = GetSideNeighborNodes(node, additionalPathSize, trueOffset, trueDirection);
        if (neighbors == null) return false;

        if (closed.Contains(neighbors[0]))
        {
            ChangeNodeColor changeNodeColor = new ChangeNodeColor(debugGrid, neighbors[0].Position, Color.green);
            commandInvoker.ExecuteCommand(changeNodeColor);
            return true;
        }

        PathInfo pathInfo = pathFunction(node, neighbors);

        if (!pathInfo.traversable)
        {
            ChangeNodeColor changeNodeColor = new ChangeNodeColor(debugGrid, neighbors[0].Position, Color.red);
            commandInvoker.ExecuteCommand(changeNodeColor);
            return false;
        }

        float newCost = node.Cost + pathInfo.cost;

        if (newCost < neighbors[0].Cost)
        {
            neighbors[0].Previous = node;
            neighbors[0].Cost = newCost;

            if (!queue.TryUpdatePriority(neighbors[0], newCost))
            {
                queue.Enqueue(neighbors[0], newCost);
            }

            ChangeNodeColor changeNodeColor = new ChangeNodeColor(debugGrid, neighbors[0].Position, Color.blue);
            ChangeNodeCost changeNodeCost = new ChangeNodeCost(debugGrid, neighbors[0].Position, newCost);
            commandInvoker.ExecuteCommand(changeNodeColor);
        }

        return true;
    }

    public List<Node> GetSideNeighborNodes(Node node, int additionalPathSize, Vector2Int offset, Vector2Int direction)
    {
        if (!grid.InBounds(node.Position + offset)) return null;

        Node neighbor = grid[node.Position + offset];
        ChangeNodeColor changeNodeColor = new ChangeNodeColor(debugGrid, neighbor.Position, Color.yellow);
        commandInvoker.ExecuteCommand(changeNodeColor);
        List<Node> neighbors = new List<Node>() { neighbor };
        Matrix4x4 rotationMatrix = FacilityGenerationManager.GetRotationMatrix(direction);

        for (int i = 1; i <= additionalPathSize; i++)
        {
            foreach (Vector2Int neighborOffset in FacilityGeneration.SideNeighbors)
            {
                Vector2 rotatedOffset = rotationMatrix * ((Vector2)neighborOffset * i);
                Vector2Int trueOffset = new Vector2Int((int)rotatedOffset.x, (int)rotatedOffset.y);

                if (!grid.InBounds(neighbor.Position + trueOffset)) return null;

                neighbors.Add(grid[neighbor.Position + trueOffset]);
            }
        }

        return neighbors;
    }

    private List<Vector2Int> ReconstructPath(Node node)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        while (node != null)
        {
            ChangeNodeColor changeNodeColor = new ChangeNodeColor(debugGrid, node.Position, Color.black);
            commandInvoker.ExecuteCommand(changeNodeColor);
            stack.Push(node.Position);
            node = node.Previous;
        }

        while (stack.Count > 0)
        {
            result.Add(stack.Pop());
        }
        return result;
    }

    public static List<Vector2Int> ExpandPath(IEnumerable<Vector2Int> path, int radius)
    {
        List<Vector2Int> neighbors = FacilityGeneration.GenerateNeighbors(radius);
        HashSet<Vector2Int> newPositions = new HashSet<Vector2Int>(path);

        foreach (Vector2Int position in path)
        {
            foreach (Vector2Int offset in neighbors)
            {
                newPositions.Add(position + offset);
            }
        }

        return new List<Vector2Int>(newPositions);
    }

    public class NodeInfo
    {
        public Color Color = Color.gray;
        public float Cost = 0;

        public NodeInfo(Color color, float cost)
        {
            Color = color;
            Cost = cost;
        }

        public void Set(Color color, float cost)
        {
            Color = color;
            Cost = cost;
        }
    }

    public class ChangeNodeColor : ICommand
    {
        Grid2D<NodeInfo> grid;
        Vector2Int position;
        Color beforeColor;
        Color color;

        public ChangeNodeColor(Grid2D<NodeInfo> grid, Vector2Int position, Color color)
        {
            this.grid = grid;
            this.position = position;
            beforeColor = grid[position].Color;
            this.color = color;
        }

        public void Execute()
        {
            grid[position].Color = color;
        }

        public void Undo()
        {
            grid[position].Color = beforeColor;
        }
    }

    public class ChangeNodeCost : ICommand
    {
        Grid2D<NodeInfo> grid;
        Vector2Int position;
        float beforeCost;
        float cost;

        public ChangeNodeCost(Grid2D<NodeInfo> grid, Vector2Int position, float cost)
        {
            this.grid = grid;
            this.position = position;
            beforeCost = grid[position].Cost;
            this.cost = cost;
        }

        public void Execute()
        {
            grid[position].Cost = cost;
        }

        public void Undo()
        {
            grid[position].Cost = beforeCost;
        }
    }

    public class ResetNodeGrid : ICommand
    {
        Grid2D<NodeInfo> beforeGrid;
        Grid2D<NodeInfo> grid;

        public ResetNodeGrid(Grid2D<NodeInfo> grid)
        {
            beforeGrid = new Grid2D<NodeInfo>(grid.Size);
            for (int x = 0; x < grid.Size.x; x++)
            {
                for (int y = 0; y < grid.Size.y; y++)
                {
                    NodeInfo info = grid[x, y];
                    beforeGrid[x, y] = new NodeInfo(info.Color, info.Cost);
                }
            }

            this.grid = grid;
        }

        public void Execute()
        {
            for (int x = 0; x < grid.Size.x; x++)
            {
                for (int y = 0; y < grid.Size.y; y++)
                {
                    grid[x, y].Set(Color.gray, 0);
                }
            }
        }

        public void Undo()
        {
            for (int x = 0; x < grid.Size.x; x++)
            {
                for (int y = 0; y < grid.Size.y; y++)
                {
                    NodeInfo info = grid[x, y];
                    grid[x, y].Set(info.Color, info.Cost);
                }
            }
        }
    }
}

public class MultiNodePathfinder2D
{
    private enum Direction
    {
        Horizontal,
        Vertical,
    }

    public class Node
    {
        private Vector2Int position;
        private Node previous;
        private float cost;

        public Vector2Int Position => position;
        public Node Previous { get { return previous; } set { previous = value; } }
        public float Cost { get { return cost; } set { cost = value; } }

        public Node(Vector2Int position)
        {
            this.position = position;
            cost = float.PositiveInfinity;
        }
    }

    private readonly struct DirectionalNode
    {
        private readonly Node node;
        private readonly Direction direction;

        public Node Node => node;
        public Direction Direction => direction;

        public DirectionalNode(Node node, Direction direction)
        {
            this.node = node;
            this.direction = direction;
        }

        public Vector2Int[] GetForwardDirections()
        {
            return direction switch
            {
                Direction.Horizontal => FacilityGeneration.SideNeighbors,
                Direction.Vertical => FacilityGeneration.ForwardNeighbors,
                _ => throw new NotImplementedException()
            };
        }

        public Vector2Int[] GetSideDirections()
        {
            return direction switch
            {
                Direction.Horizontal => FacilityGeneration.ForwardNeighbors,
                Direction.Vertical => FacilityGeneration.SideNeighbors,
                _ => throw new NotImplementedException()
            };
        }

        public Direction GetRotatedDirection()
        {
            return (Direction)(((int)direction + 1) % 2);
        }
    }

    public struct EvaluationInfo
    {
        public bool Traversable;
        public float Cost;
    }

    private Grid2D<Node> nodeGrid;
    private SimplePriorityQueue<DirectionalNode, float> queue;
    private HashSet<DirectionalNode> closed;
    private Dictionary<DirectionalNode, bool> evaluated;
    private Stack<Vector2Int> stack;

    private Grid2D<VisualNode> visualNodeGrid;
    private CommandInvoker commandInvoker;

    public Grid2D<VisualNode> VisualNodeGrid => visualNodeGrid;
    public CommandInvoker CommandInvoker => commandInvoker;

    public MultiNodePathfinder2D(Vector2Int size)
    {
        nodeGrid = new Grid2D<Node>(size);

        queue = new SimplePriorityQueue<DirectionalNode, float>();
        closed = new HashSet<DirectionalNode>();
        evaluated = new Dictionary<DirectionalNode, bool>();
        stack = new Stack<Vector2Int>();

        visualNodeGrid = new Grid2D<VisualNode>(size);
        commandInvoker = new CommandInvoker();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                nodeGrid[x, y] = new Node(new Vector2Int(x, y));
                visualNodeGrid[x, y] = new VisualNode(Color.white, 0);
            }
        }
    }

    private void ResetNodes()
    {
        foreach (Node node in nodeGrid.Data)
        {
            node.Previous = null;
            node.Cost = float.PositiveInfinity;
        }

        ResetNodeGrid resetNodeGrid = new ResetNodeGrid(visualNodeGrid);
        commandInvoker.ExecuteCommand(resetNodeGrid);
    }

    private void ResetPathfinder(Vector2Int start)
    {
        ResetNodes();
        queue.Clear();
        closed.Clear();
        evaluated.Clear();

        nodeGrid[start].Cost = 0;
        queue.Enqueue(new DirectionalNode(nodeGrid[start], Direction.Horizontal), 0);
        queue.Enqueue(new DirectionalNode(nodeGrid[start], Direction.Vertical), 0);
    }

    public List<Vector2Int> FindPath(Vector2Int start, int pathSize, Func<Node, List<Node>, EvaluationInfo> evaluationFunction, Func<Node, bool> goalFunction)
    {
        ResetPathfinder(start);

        while (queue.Count > 0)
        {
            DirectionalNode directionalNode = queue.Dequeue();
            closed.Add(directionalNode);

            ChangeNodeColor changeNodeColor = new ChangeNodeColor(visualNodeGrid, directionalNode.Node.Position, Color.green);
            commandInvoker.ExecuteCommand(changeNodeColor);

            //evaluated.Add(directionalNode, true);

            if (goalFunction(directionalNode.Node)) return ReconstructPath(directionalNode.Node);

            EvaluateNeighbors(directionalNode, start, pathSize, evaluationFunction);
        }

        return null;
    }

    private void EvaluateNeighbors(DirectionalNode originDirectionalNode, Vector2Int start, int pathSize, Func<Node, List<Node>, EvaluationInfo> evaluationFunction)
    {
        bool success = true;
        foreach (Vector2Int direction in originDirectionalNode.GetForwardDirections())
        {
            for (int i = 1; i <= pathSize; i++)
            {
                Vector2Int offset = direction * i;
                if (!EvaluateSideNeighbors(originDirectionalNode, start, pathSize, (i == 1), offset, evaluationFunction))
                {
                    success = false;
                    break;
                }
            }
        }

        if (!success) return;

        foreach (Vector2Int direction in originDirectionalNode.GetSideDirections())
        {
            EvaluateSideNeighbors(new DirectionalNode(originDirectionalNode.Node, originDirectionalNode.GetRotatedDirection()), start, pathSize, true, direction, evaluationFunction);
        }
    }

    private bool EvaluateSideNeighbors(DirectionalNode originDirectionalNode, Vector2Int start, int pathSize, bool queueNode, Vector2Int offset, Func<Node, List<Node>, EvaluationInfo> evaluationFunction)
    {
        if (!nodeGrid.InBounds(originDirectionalNode.Node.Position + offset)) return false;
        Node neighbor = nodeGrid[originDirectionalNode.Node.Position + offset];
        DirectionalNode directionalNeighbor = new DirectionalNode(neighbor, originDirectionalNode.Direction);

        if (closed.Contains(directionalNeighbor))
        {
            return true;
        }

        if (!queueNode)
        {
            if (evaluated.TryGetValue(directionalNeighbor, out bool passedEvaluation))
            {
                return passedEvaluation;
            }
        }

        ChangeNodeColor changeNodeColor = new ChangeNodeColor(visualNodeGrid, directionalNeighbor.Node.Position, Color.yellow);
        commandInvoker.ExecuteCommand(changeNodeColor);

        List<Node> neighbors = GetSideNeighborNodes(originDirectionalNode, neighbor, pathSize);
        if (neighbors == null)
        {
            changeNodeColor = new ChangeNodeColor(visualNodeGrid, directionalNeighbor.Node.Position, Color.red);
            commandInvoker.ExecuteCommand(changeNodeColor);

            evaluated.TryAdd(directionalNeighbor, false);
            return false;
        }

        EvaluationInfo evaluationInfo = evaluationFunction(originDirectionalNode.Node, neighbors);

        if (!evaluationInfo.Traversable)
        {
            changeNodeColor = new ChangeNodeColor(visualNodeGrid, directionalNeighbor.Node.Position, Color.red);
            commandInvoker.ExecuteCommand(changeNodeColor);

            evaluated.TryAdd(directionalNeighbor, false);
            return false;
        }

        if (queueNode)
        {
            float dst = Mathf.Pow(Vector2Int.Distance(neighbor.Position, originDirectionalNode.Node.Position), 2);
            float nodeCost = dst + originDirectionalNode.Node.Cost;
            float newCost = nodeCost + evaluationInfo.Cost;

            if (newCost < neighbor.Cost)
            {
                ChangeNodeCost changeNodeCost = new ChangeNodeCost(visualNodeGrid, neighbor.Position, newCost);
                commandInvoker.ExecuteCommand(changeNodeCost);

                neighbor.Previous = originDirectionalNode.Node;
                neighbor.Cost = nodeCost;

                if (!queue.TryUpdatePriority(directionalNeighbor, newCost))
                {
                    queue.Enqueue(directionalNeighbor, newCost);
                }
            }
            changeNodeColor = new ChangeNodeColor(visualNodeGrid, directionalNeighbor.Node.Position, Color.blue);
            commandInvoker.ExecuteCommand(changeNodeColor);
        }
        else
        {
            changeNodeColor = new ChangeNodeColor(visualNodeGrid, directionalNeighbor.Node.Position, Color.cyan);
            commandInvoker.ExecuteCommand(changeNodeColor);
        }

        evaluated.TryAdd(directionalNeighbor, true);
        return true;
    }

    private List<Node> GetSideNeighborNodes(DirectionalNode originDirectionalNode, Node neighbor, int pathSize)
    {
        List<Node> neighbors = new List<Node>() { neighbor };

        foreach (Vector2Int direction in originDirectionalNode.GetSideDirections())
        {
            for (int i = 1; i <= pathSize; i++)
            {
                Vector2Int neighborPosition = neighbor.Position + direction * i;
                if (!nodeGrid.InBounds(neighborPosition)) return null;

                neighbors.Add(nodeGrid[neighborPosition]);
            }
        }

        return neighbors;
    }

    private List<Vector2Int> ReconstructPath(Node node)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        while (node != null)
        {
            ChangeNodeColor changeNodeColor = new ChangeNodeColor(visualNodeGrid, node.Position, Color.magenta);
            commandInvoker.ExecuteCommand(changeNodeColor);
            stack.Push(node.Position);
            node = node.Previous;
        }

        while (stack.Count > 0)
        {
            result.Add(stack.Pop());
        }
        return result;
    }

    public static List<Vector2Int> ExpandPath(IEnumerable<Vector2Int> path, int radius)
    {
        List<Vector2Int> neighbors = FacilityGeneration.GenerateNeighbors(radius);
        HashSet<Vector2Int> newPositions = new HashSet<Vector2Int>(path);

        foreach (Vector2Int position in path)
        {
            foreach (Vector2Int offset in neighbors)
            {
                newPositions.Add(position + offset);
            }
        }

        return new List<Vector2Int>(newPositions);
    }

    public class VisualNode
    {
        public float Cost = 0;
        public Color Color = Color.white;

        public VisualNode(Color color, float cost)
        {
            Color = color;
            Cost = cost;
        }

        public void Set(Color color, float cost)
        {
            Color = color;
            Cost = cost;
        }
    }

    public class ChangeNodeColor : ICommand
    {
        Grid2D<VisualNode> grid;
        Vector2Int position;
        Color beforeColor;
        Color color;

        public ChangeNodeColor(Grid2D<VisualNode> grid, Vector2Int position, Color color)
        {
            this.grid = grid;
            this.position = position;
            beforeColor = grid[position].Color;
            this.color = color;
        }

        public void Execute()
        {
            grid[position].Color = color;
        }

        public void Undo()
        {
            grid[position].Color = beforeColor;
        }
    }

    public class ChangeNodeCost : ICommand
    {
        Grid2D<VisualNode> grid;
        Vector2Int position;
        float beforeCost;
        float cost;

        public ChangeNodeCost(Grid2D<VisualNode> grid, Vector2Int position, float cost)
        {
            this.grid = grid;
            this.position = position;
            beforeCost = grid[position].Cost;
            this.cost = cost;
        }

        public void Execute()
        {
            grid[position].Cost = cost;
        }

        public void Undo()
        {
            grid[position].Cost = beforeCost;
        }
    }

    public class ResetNodeGrid : ICommand
    {
        Grid2D<VisualNode> beforeGrid;
        Grid2D<VisualNode> grid;

        public ResetNodeGrid(Grid2D<VisualNode> grid)
        {
            beforeGrid = new Grid2D<VisualNode>(grid.Size);
            for (int x = 0; x < grid.Size.x; x++)
            {
                for (int y = 0; y < grid.Size.y; y++)
                {
                    VisualNode info = grid[x, y];
                    beforeGrid[x, y] = new VisualNode(info.Color, info.Cost);
                }
            }

            this.grid = grid;
        }

        public void Execute()
        {
            for (int x = 0; x < grid.Size.x; x++)
            {
                for (int y = 0; y < grid.Size.y; y++)
                {
                    grid[x, y].Set(Color.gray, 0);
                }
            }
        }

        public void Undo()
        {
            for (int x = 0; x < grid.Size.x; x++)
            {
                for (int y = 0; y < grid.Size.y; y++)
                {
                    VisualNode info = grid[x, y];
                    grid[x, y].Set(info.Color, info.Cost);
                }
            }
        }
    }
}