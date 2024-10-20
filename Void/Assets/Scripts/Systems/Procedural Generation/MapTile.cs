using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    private MapTileCollection parentCollection;
    private Vector2Int position;

    public MapTileCollection ParentCollection { get { return parentCollection; } }
    public Vector2Int Position {  get { return position; } }

    public void SetMapTile(MapTileCollection parentCollection, Vector2Int position)
    {
        this.parentCollection = parentCollection;
        this.position = position;
    }

    public void Spawn()
    {
        GridMapManager gridMapManager = GridMapManager.Instance;
        float mapTileSize = gridMapManager.TileSize;
        GameObject floor = Instantiate(gridMapManager.Floor, transform.position, Quaternion.identity, transform);
        floor.transform.localScale = mapTileSize * Vector3.one;

        bool isHallway = parentCollection.MapTileCollectionType == MapTileCollectionType.Hallway;

        MapTile hasNorthWall = gridMapManager.GetMapTile(position + new Vector2Int(0, 1));
        MapTile hasEastWall = gridMapManager.GetMapTile(position + new Vector2Int(1, 0));
        MapTile hasSouthWall = gridMapManager.GetMapTile(position + new Vector2Int(0, -1));
        MapTile hasWestWall = gridMapManager.GetMapTile(position + new Vector2Int(-1, 0));

        if (hasNorthWall)
        {
            if (hasNorthWall.parentCollection.Id != parentCollection.Id && !(isHallway && hasNorthWall.parentCollection.MapTileCollectionType == MapTileCollectionType.Hallway))
            {
                GameObject wall = Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 0, 0), transform);
                wall.transform.localScale = mapTileSize * Vector3.one;
            }
        }
        else
        {
            GameObject wall = Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 0, 0), transform);
            wall.transform.localScale = mapTileSize * Vector3.one;
        }
        if (hasEastWall)
        {
            if (hasEastWall.parentCollection.Id != parentCollection.Id && !(isHallway && hasEastWall.parentCollection.MapTileCollectionType == MapTileCollectionType.Hallway))
            {
                GameObject wall = Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 90, 0), transform);
                wall.transform.localScale = mapTileSize * Vector3.one;
            }
        }
        else
        {
            GameObject wall = Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 90, 0), transform);
            wall.transform.localScale = mapTileSize * Vector3.one;
        }
        if (hasSouthWall)
        {
            if (hasSouthWall.parentCollection.Id != parentCollection.Id && !(isHallway && hasSouthWall.parentCollection.MapTileCollectionType == MapTileCollectionType.Hallway))
            {
                GameObject wall = Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 180, 0), transform);
                wall.transform.localScale = mapTileSize * Vector3.one;
            }
        }
        else
        {
            GameObject wall = Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 180, 0), transform);
            wall.transform.localScale = mapTileSize * Vector3.one;
        }
        if (hasWestWall)
        {
            if (hasWestWall.parentCollection.Id != parentCollection.Id && !(isHallway && hasWestWall.parentCollection.MapTileCollectionType == MapTileCollectionType.Hallway))
            {
                GameObject wall = Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 270, 0), transform);
                wall.transform.localScale = mapTileSize * Vector3.one;
            }
        }
        else
        {
            GameObject wall = Instantiate(gridMapManager.Wall, transform.position, Quaternion.Euler(0, 270, 0), transform);
            wall.transform.localScale = mapTileSize * Vector3.one;
        }
    }
}
