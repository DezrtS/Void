using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
    public Vector2 A, B, C;
    public bool IsBad;


    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        A = a;
        B = b;
        C = c;
        IsBad = false;
    }

    public bool ContainsPoint(Vector2 point)
    {
        return point == A || point == B || point == C;
    }

    public bool CircumcircleContains(Vector2 point)
    {
        float ax = A.x - point.x;
        float ay = A.y - point.y;
        float bx = B.x - point.x;
        float by = B.y - point.y;
        float cx = C.x - point.x;
        float cy = C.y - point.y;

        float det = (ax * ax + ay * ay) * (bx * cy - by * cx)
                  - (bx * bx + by * by) * (ax * cy - ay * cx)
                  + (cx * cx + cy * cy) * (ax * by - ay * bx);

        return det < 0;
    }

    public static bool operator ==(Triangle left, Triangle right)
    {
        return (left.A == right.A || left.A == right.B || left.A == right.C)
            && (left.B == right.A || left.B == right.B || left.B == right.C)
            && (left.C == right.A || left.C == right.B || left.C == right.C);
    }

    public static bool operator !=(Triangle left, Triangle right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is Triangle t)
        {
            return this == t;
        }

        return false;
    }

    public bool Equals(Triangle t)
    {
        return this == t;
    }

    public override int GetHashCode()
    {
        return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
    }
}

public class Edge : IEquatable<Edge>
{
    public Vector2 A { get; }
    public Vector2 B { get; }
    public bool IsBad { get; set; }

    public Edge(Vector2 a, Vector2 b)
    {
        A = a;
        B = b;
        IsBad = false;
    }

    public static bool operator ==(Edge left, Edge right)
    {
        return (left.A == right.A && left.B == right.B) || (left.A == right.B && left.B == right.A);
    }

    public static bool operator !=(Edge left, Edge right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        return obj is Edge edge && this == edge;
    }

    public bool Equals(Edge other)
    {
        return this == other;
    }

    public override int GetHashCode()
    {
        return A.GetHashCode() ^ B.GetHashCode();
    }

    public float Length()
    {
        return Vector2.Distance(A, B);
    }
}

public class Delaunay
{
    public List<Vector2> Points;
    public List<Edge> Edges;
    public List<Triangle> Triangles;

    public Delaunay()
    {
        Edges = new List<Edge> ();
        Triangles = new List<Triangle> ();
    }

    public static Delaunay Triangulate(List<Vector2> points)
    {
        Delaunay delaunay = new Delaunay();
        delaunay.Points = new List<Vector2>(points);
        delaunay.Triangulate();

        return delaunay;
    }

    private void Triangulate()
    {
        Triangle superTriangle = CreateSuperTriangle(Points);
        Triangles.Add(superTriangle);

        foreach (Vector2 point in Points)
        {
            UpdateTriangulation(point);
        }

        Triangles.RemoveAll((Triangle t) => t.ContainsPoint(superTriangle.A) || t.ContainsPoint(superTriangle.B) || t.ContainsPoint(superTriangle.C));

        HashSet<Edge> edgeSet = new HashSet<Edge>();

        foreach (var t in Triangles)
        {
            var ab = new Edge(t.A, t.B);
            var bc = new Edge(t.B, t.C);
            var ca = new Edge(t.C, t.A);

            if (edgeSet.Add(ab))
            {
                Edges.Add(ab);
            }

            if (edgeSet.Add(bc))
            {
                Edges.Add(bc);
            }

            if (edgeSet.Add(ca))
            {
                Edges.Add(ca);
            }
        }
    }

    private static Triangle CreateSuperTriangle(List<Vector2> points)
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (Vector2 point in points)
        {
            if (point.x < minX)
            {
                minX = point.x;
            }
            if (point.x > maxX)
            {
                maxX = point.x;
            }
            if (point.y < minY)
            {
                minY = point.y;
            }
            if (point.y > maxY)
            {
                maxY = point.y;
            }
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float deltaMax = Mathf.Max(dx, dy);
        Vector2 p1 = new(minX - deltaMax, minY - deltaMax);
        Vector2 p2 = new(minX + dx / 2f, maxY + deltaMax);
        Vector2 p3 = new(maxX + deltaMax, minY - deltaMax);

        return new Triangle(p1, p2, p3);
    }

    private void UpdateTriangulation(Vector2 point)
    {
        List<Edge> polygonalHole = new List<Edge>();
        foreach (Triangle triangle in Triangles)
        {
            if (triangle.CircumcircleContains(point))
            {
                triangle.IsBad = true;
                polygonalHole.Add(new Edge(triangle.A, triangle.B));
                polygonalHole.Add(new Edge(triangle.B, triangle.C));
                polygonalHole.Add(new Edge(triangle.C, triangle.A));
            }
        }

        Triangles.RemoveAll((Triangle t) => t.IsBad);


        for (int i = 0; i < polygonalHole.Count; i++)
        {
            for (int j = i + 1; j < polygonalHole.Count; j++)
            {
                if (polygonalHole[i] == polygonalHole[j])
                {
                    polygonalHole[i].IsBad = true;
                    polygonalHole[j].IsBad = true;
                }
            }
        }

        polygonalHole.RemoveAll((Edge e) => e.IsBad);

        foreach (Edge e in polygonalHole)
        {
            Triangles.Add(new Triangle(e.A, e.B, point));
        }
    }
}