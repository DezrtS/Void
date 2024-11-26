using UnityEngine;

public class Grid2D<T>
{
    readonly T[] data;

    public Vector2Int Size { get; private set; }
    public Vector2Int Offset { get; set; }

    public Grid2D(Vector2Int size, Vector2Int offset)
    {
        Size = size;
        Offset = offset;

        data = new T[size.x * size.y];
    }

    public int GetIndex(Vector2Int pos)
    {
        return pos.x + (Size.x * pos.y);
    }

    public bool InBounds(Vector2Int pos)
    {
        return new RectInt(Vector2Int.zero, Size).Contains(pos + Offset);
    }

    public T this[int x, int y]
    {
        get
        {
            return this[new Vector2Int(x, y)];
        }
        set
        {
            this[new Vector2Int(x, y)] = value;
        }
    }

    public T this[Vector2Int pos]
    {
        get
        {
            pos += Offset;
            return data[GetIndex(pos)];
        }
        set
        {
            pos += Offset;
            data[GetIndex(pos)] = value;
        }
    }

    public T this[Vector2 pos]
    {
        get
        {
            pos += Offset;
            return this[(int)pos.x, (int)pos.y];
        }
    }

    public T this[int index]
    {
        get
        {
            return data[index];
        }
        set
        {
            data[index] = value;
        }
    }
}