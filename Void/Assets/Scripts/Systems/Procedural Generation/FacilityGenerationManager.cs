using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class FacilityGenerationManager : Singleton<FacilityGenerationManager>
{
    [Header("Facility Generation Parameters")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private int seed = 1234;
    [SerializeField] private Vector2Int size = new Vector2Int(25, 25);
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

    private EdgeVisualizer edgeVisualizer;

    private GameObject levelHolder;

    public GameObject DebugMarker => debugMarker;
    public EdgeVisualizer EdgeVisualizer => edgeVisualizer;
         
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

        edgeVisualizer = GetComponent<EdgeVisualizer>();
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
        edgeVisualizer.ClearLines();
        Destroy(levelHolder);
        facilityFloor = new FacilityFloor(seed, size, Vector3.zero, tileSize);
    }

    public void GenerateFacilityFloor()
    {
        ResetFacilityFloor();
        Random.InitState(seed);
        levelHolder = new GameObject("Facility Floor");

        roomGenerator.GenerateRooms(facilityFloor);
        hallwayGenerator.GenerateHallways(facilityFloor);
        interiorGenerator.GenerateInteriors(facilityFloor);

        SpawnTiles();
        SpawnInteriors();
    }

    public void SpawnTiles()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                FacilityGeneration.TileType type = facilityFloor.TileMap[x, y].Type;
                GameObject newTile = new GameObject("Tile");
                newTile.transform.parent = levelHolder.transform;

                if (type == FacilityGeneration.TileType.Room)
                {
                    Instantiate(floor, new Vector3(x, 0, y), Quaternion.identity, newTile.transform);
                }
                else if (type == FacilityGeneration.TileType.Hallway)
                {
                    Instantiate(floor, new Vector3(x, 0, y), Quaternion.identity, newTile.transform);
                }
                else if (type == FacilityGeneration.TileType.Walkway)
                {
                    Instantiate(floor, new Vector3(x, 0, y), Quaternion.identity, newTile.transform);
                }
                else
                {
                    Instantiate(wall, new Vector3(x, 0, y), Quaternion.identity, newTile.transform);
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
                float rotation = 90 * (int)fixtureInstance.RotationPreset;
                /*GameObject fixture = */
                Instantiate(fixtureInstance.Data.FixturePrefab, tileSize * new Vector3(fixtureInstance.Position.x, 0, fixtureInstance.Position.y), Quaternion.Euler(0, rotation, 0), levelHolder.transform);
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

    public static Matrix4x4 GetRotationMatrix(Vector2Int direction)
    {
        RotationPreset rotationPreset = RotationPreset.Zero;

        if (direction == Vector2Int.right)
        {
            rotationPreset = RotationPreset.Ninety;
        }
        else if (direction == Vector2Int.down)
        {
            rotationPreset = RotationPreset.OneEighty;
        }
        else if (direction == Vector2Int.left)
        {
            rotationPreset = RotationPreset.TwoSeventy;
        }

        return GetRotationMatrix(rotationPreset);
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