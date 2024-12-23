using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FacilityGenerationManager : Singleton<FacilityGenerationManager>
{
    [Header("Facility Generation Parameters")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private int seed = 1234;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(25, 25);
    [SerializeField] private float tileSize = 10;

    [Header("Prefabs")]
    [SerializeField] private GameObject floor;
    [SerializeField] private GameObject wall;

    [Header("Debugging")]
    [SerializeField] private GameObject debugMarker;

    private FacilityFloor facilityFloor;
    private RoomGenerator roomGenerator;
    private HallwayGenerator hallwayGenerator;
    private InteriorGenerator interiorGenerator;

    private GameObject levelHolder;

    public GameObject DebugMarker => debugMarker;

    // TODO List
    // TODO - Fix fixtures spawn through walls when interior tiles per map tile is even
    // TODO - Fix Generate Walkway method results in out of bounds error for hallway tile collection
    // TODO - Uncomment and fix fixture restriction verification
    // TODO - Create debugging functionality with procedurally generation

    private void Awake()
    {
        roomGenerator = GetComponent<RoomGenerator>();
        hallwayGenerator = GetComponent<HallwayGenerator>();
        interiorGenerator = GetComponent<InteriorGenerator>();
    }

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateFacilityFloor();
        }
    }

    public void ResetFacilityFloor()
    {
        Destroy(levelHolder);
        facilityFloor = new FacilityFloor(seed, gridSize, Vector3.zero, tileSize, interiorGenerator.InteriorTilesPerMapTile);
    }

    public void GenerateFacilityFloor()
    {
        ResetFacilityFloor();
        Random.InitState(seed);
        levelHolder = new GameObject("Facility Floor");

        roomGenerator.GenerateRooms(facilityFloor);
        hallwayGenerator.GenerateHallways(facilityFloor);
        SpawnMapTiles();

        interiorGenerator.GenerateInteriors(facilityFloor);
        SpawnInteriors();
    }

    public void SpawnMapTiles()
    {
        Vector2Int[] neighbors =
{
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        foreach (TileCollection tileCollection in facilityFloor.TileCollections)
        {
            RoomData roomData = tileCollection.RoomData;
            bool spawnFloors = true;
            bool spawnWalls = true;
            if (roomData)
            {
                spawnFloors = roomData.SpawnFloors;
                spawnWalls = roomData.SpawnWalls;

                if (!spawnFloors && !spawnWalls)
                {
                    continue;
                }

            }

            foreach (Vector2Int position in tileCollection.mapTilePositions)
            {
                GameObject newTile = new GameObject("Tile");
                newTile.transform.parent = levelHolder.transform;
                if (spawnFloors)
                {
                    Instantiate(floor, new Vector3(position.x, 0, position.y), Quaternion.identity, newTile.transform);
                }

                if (spawnWalls)
                {
                    foreach (Vector2Int neighbor in neighbors)
                    {
                        if (facilityFloor.FloorMap.InBounds(position + neighbor))
                        {
                            if (tileCollection.connections.Contains(new Connection(position, position + neighbor)))
                            {
                                continue;
                            }

                            if (facilityFloor.FloorMap[position].Collection == facilityFloor.FloorMap[position + neighbor].Collection)
                            {
                                continue;
                            }
                        }

                        float angle = 0;
                        if (neighbor == Vector2Int.up)
                        {
                            angle = 0;
                        }
                        else if (neighbor == Vector2Int.right)
                        {
                            angle = 90;
                        }
                        else if (neighbor == Vector2Int.down)
                        {
                            angle = 180;
                        }
                        else
                        {
                            angle = 270;
                        }

                        Instantiate(wall, new Vector3(position.x, 0, position.y), Quaternion.Euler(0, angle, 0), newTile.transform);
                    }
                }

                newTile.transform.localScale = tileSize * Vector3.one;
            }
        }
    }

    public void SpawnInteriors()
    {
        foreach (TileCollection tileCollection in facilityFloor.TileCollections)
        {
            foreach (FixtureInstance fixtureInstance in tileCollection.FixtureInstances)
            {
                Vector2 spawnPosition = ((Vector2)(fixtureInstance.Position) - (facilityFloor.InteriorTilesPerMapTile / 2) * Vector2.one) / facilityFloor.InteriorTilesPerMapTile;
                float rotation = 90 * (int)fixtureInstance.RotationPreset;
                /*GameObject fixture = */
                Instantiate(fixtureInstance.Data.FixturePrefab, tileSize * new Vector3(spawnPosition.x, 0, spawnPosition.y), Quaternion.Euler(0, rotation, 0), levelHolder.transform);
                //fixture.AddComponent<DebugFixture>().SetupValues(fixtureInstance);

                //if (spawnFixturePositions)
                //{
                //    foreach (Vector2Int position in fixtureInstance.Data.TilePositions)
                //    {
                //        Vector3 rotatedPosition = fixtureInstance.RotationMatrix * (Vector2)position;
                //        Vector2 newSpawnPosition = (((Vector2)rotatedPosition + fixtureInstance.Position) - (interiorTilesPerMapTile / 2) * Vector2.one) / interiorTilesPerMapTile;
                //        Instantiate(debugMarker, tileSize * new Vector3(newSpawnPosition.x, 0, newSpawnPosition.y), Quaternion.identity, levelHolder.transform);
                //    }
                //}
            }
        }
    }

    public static bool Roll(float chance)
    {
        return Random.Range(0, 100) < chance;
    }

    public static Matrix4x4 GetRotationMatrix(RotationPreset rotationPreset)
    {
        return rotationPreset switch
        {
            RotationPreset.Zero => new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            ),
            RotationPreset.Ninety => new Matrix4x4(
                new Vector4(0, -1, 0, 0),
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            ),
            RotationPreset.OneEighty => new Matrix4x4(
                new Vector4(-1, 0, 0, 0),
                new Vector4(0, -1, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            ),
            RotationPreset.TwoSeventy => new Matrix4x4(
                new Vector4(0, 1, 0, 0),
                new Vector4(-1, 0, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            ),
            _ => new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            ),
        };
    }
}