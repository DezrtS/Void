using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    private MapTileCollection parentCollection;
    private Vector2 position;

    public Vector2 Position {  get { return position; } }

    public void SetMapTile(MapTileCollection parentCollection, Vector2 position)
    {
        this.parentCollection = parentCollection;
        this.position = position;
    }

    public void Spawn()
    {
        GridMapManager gridMapManager = GridMapManager.Instance;
        float mapTileSize = gridMapManager.MapTileSize;
        Instantiate(gridMapManager.Floor, transform.position, Quaternion.identity, transform);

        MapTile hasNorthWall = gridMapManager.GetMapTile(position + new Vector2(0, 1));
        MapTile hasEastWall = gridMapManager.GetMapTile(position + new Vector2(1, 0));
        MapTile hasSouthWall = gridMapManager.GetMapTile(position + new Vector2(0, -1));
        MapTile hasWestWall = gridMapManager.GetMapTile(position + new Vector2(-1, 0));

        if (hasNorthWall == null || hasNorthWall.parentCollection.Id != parentCollection.Id)
        {
            Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 0, 0), transform);
        }
        if (hasEastWall == null || hasEastWall.parentCollection.Id != parentCollection.Id)
        {
            Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 90, 0), transform);
        }
        if (hasSouthWall == null || hasSouthWall.parentCollection.Id != parentCollection.Id)
        {
            Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 180, 0), transform);
        }
        if (hasWestWall == null || hasWestWall.parentCollection.Id != parentCollection.Id)
        {
            Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 270, 0), transform);
        }
    }
}
