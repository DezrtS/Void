using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class FixtureGeneration
{
    public static bool CheckTiles(FacilityFloor facilityFloor, IEnumerable<Vector2Int> positions)
    {
        foreach (var position in positions)
        {
            if (facilityFloor.FixtureMap[position] != null) return false;
        }

        return true;
    }

    public static List<Vector2Int> GetFixtureTilePositions(FixtureInstance fixtureInstance)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        foreach (Vector2Int position in fixtureInstance.Data.TilePositions)
        {
            Vector2 rotatedPosition = FacilityGenerationManager.GetRotationMatrix(fixtureInstance.RotationPreset) * (Vector2)position;
            Vector2Int newPosition = new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y) + fixtureInstance.Position;

            positions.Add(newPosition);
        }

        return positions;
    }

    public static FixtureInstance RandomlyPlaceFixture(FacilityFloor facilityFloor, FixtureData fixtureData, TileCollection tileCollection)
    {
        Vector2Int randomPosition = tileCollection.GetRandomTilePosition();
        RotationPreset randomRotationPreset = (RotationPreset)Random.Range(0, 4);
        Matrix4x4 rotationMatrix = FacilityGenerationManager.GetRotationMatrix(randomRotationPreset);

        FixtureInstance fixtureInstance = new FixtureInstance(fixtureData, tileCollection, randomPosition, rotationMatrix, randomRotationPreset);

        if (TryPlaceFixture(facilityFloor, tileCollection, fixtureInstance)) return fixtureInstance;

        return null;
    }

    public static void PlaceFixture(FacilityFloor facilityFloor, TileCollection tileCollection, FixtureInstance fixtureInstance, IEnumerable<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
        {
            facilityFloor.FixtureMap[position] = fixtureInstance;
        }

        tileCollection.AddFixtureInstance(fixtureInstance);
    }

    public static bool TryPlaceFixture(FacilityFloor facilityFloor, TileCollection tileCollection, FixtureInstance fixtureInstance)
    {
        List<Vector2Int> positions = GetFixtureTilePositions(fixtureInstance);

        foreach (Vector2Int position in positions)
        {
            if (!facilityFloor.TileMap.InBounds(position)) return false;
            if (facilityFloor.TileMap[position].TileCollection != tileCollection) return false;
        }

        if (!FacilityGeneration.CheckTiles(facilityFloor, positions, FacilityGeneration.TileType.Room)) return false;
        if (!CheckTiles(facilityFloor, positions)) return false;

        if (!VerifyFixtureRestrictions(facilityFloor, tileCollection, fixtureInstance, positions)) return false;

        PlaceFixture(facilityFloor, tileCollection, fixtureInstance, positions);
        
        //if (!VerifyAllFixtureRestrictions(facilityFloor, tileCollection, fixtureInstance, positions))
        //{
        //    InteriorGeneration.PlaceInteriorTiles(facilityFloor, null, InteriorTile.InteriorTileType.None, positions);
        //    return false;
        //}

        return true;
    }

    //public static bool VerifyFixtureRestrictions(FacilityFloor facilityFloor, TileCollection tileCollection, FixtureInstance fixtureInstance, FixtureInstance newFixtureInstance, IEnumerable<Vector2Int> newFixturePositions)
    //{
    //    if (fixtureInstance.Data.Restrictions.Count == 0) return true;

    //    bool allDisabled = true;
    //    foreach (RestrictionData restrictionData in fixtureInstance.Data.Restrictions)
    //    {
    //        if (!restrictionData.Enabled) continue;
    //        allDisabled = false;

    //        bool manditoryVsProhibited;
    //        bool condition = true;
    //        foreach (Restriction restriction in restrictionData.Restrictions)
    //        {
    //            manditoryVsProhibited = restriction.Type == Restriction.RestrictionType.Manditory;
    //            condition = true;

    //            if (restriction.HasInteriorTileType && condition)
    //            {
    //                foreach (Vector2Int position in restriction.Positions)
    //                {
    //                    Vector3 rotatedPosition = fixtureInstance.RotationMatrix * (Vector2)position;
    //                    Vector2Int offsetPosition = fixtureInstance.Position + new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);

    //                    if (newFixturePositions.Contains(offsetPosition))
    //                    {
    //                        if (restriction.InteriorTileType == InteriorTile.InteriorTileType.Fixture)
    //                        {
    //                            if (!manditoryVsProhibited)
    //                            {
    //                                condition = false;
    //                                break;
    //                            }
    //                        }
    //                        else
    //                        {
    //                            if (manditoryVsProhibited)
    //                            {
    //                                condition = false;
    //                                break;
    //                            }
    //                        }
    //                    }
    //                }
    //            }

    //            if (restriction.HasPathToWalkableTile && condition)
    //            {
    //                Vector2Int size = tileCollection.Bounds.upperRight - tileCollection.Bounds.lowerLeft;
    //                Pathfinder2D pathfinder2D = new Pathfinder2D(size * facilityFloor.InteriorTilesPerMapTile);

    //                foreach (Vector2Int position in restriction.Positions)
    //                {
    //                    Vector3 rotatedPosition = fixtureInstance.RotationMatrix * (Vector2)position;
    //                    Vector2Int offsetPosition = fixtureInstance.Position + new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
    //                    if (!facilityFloor.InteriorFloorMap.InBounds(offsetPosition))
    //                    {
    //                        if (manditoryVsProhibited)
    //                        {
    //                            condition = false;
    //                            break;
    //                        }
    //                    }

    //                    float minDistance = float.MaxValue;
    //                    Vector2Int closestPosition = offsetPosition;
    //                    foreach (Connection connection in tileCollection.connections)
    //                    {
    //                        float distance = (connection.from - offsetPosition).magnitude;
    //                        if (distance <= minDistance)
    //                        {
    //                            closestPosition = connection.from;
    //                            minDistance = distance;
    //                        }
    //                    }

    //                    List<Vector2Int> path = pathfinder2D.FindPath(offsetPosition, closestPosition, (Pathfinder2D.Node a, Pathfinder2D.Node b) =>
    //                    {
    //                        Pathfinder2D.PathInfo pathInfo = new Pathfinder2D.PathInfo();

    //                        pathInfo.cost = Vector2Int.Distance(b.Position, closestPosition);
    //                        pathInfo.traversable = true;

    //                        if (newFixturePositions.Contains(b.Position))
    //                        {
    //                            pathInfo.traversable = false;
    //                            return pathInfo;
    //                        }

    //                        InteriorTile interiorTile = facilityFloor.InteriorFloorMap[b.Position];

    //                        if (interiorTile.Type == InteriorTile.InteriorTileType.None)
    //                        {
    //                            pathInfo.cost += 5;
    //                        }
    //                        else if (interiorTile.Type == InteriorTile.InteriorTileType.Walkway)
    //                        {
    //                            pathInfo.traversable = false;
    //                            pathInfo.isGoal = true;
    //                        }
    //                        else
    //                        {
    //                            pathInfo.traversable = false;
    //                        }

    //                        return pathInfo;
    //                    });

    //                    if (path.Count == 0)
    //                    {
    //                        if (manditoryVsProhibited)
    //                        {
    //                            condition = false;
    //                            break;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (!manditoryVsProhibited)
    //                        {
    //                            condition = false;
    //                            break;
    //                        }
    //                    }
    //                }
    //            }

    //            if (!condition)
    //            {
    //                break;
    //            }
    //        }

    //        if (condition)
    //        {
    //            return true;
    //        }
    //    }

    //    if (allDisabled)
    //    {
    //        return true;
    //    }

    //    return false;
    //}

    // TODO - Implement optimization (SEND POSITIONS TO VERIFY FUNCTION TO CHECK IF THOSE POSITIONS ARE INSIDE OF THE BOUNDS OF THE RESTRICTIONS AND IF SO, IF THEY NEGATE ITS RESTRICTIONS)
    public static bool VerifyFixtureRestrictions(FacilityFloor facilityFloor, TileCollection tileCollection, FixtureInstance fixtureInstance, IEnumerable<Vector2Int> positions)
    {
        if (fixtureInstance.Data.Restrictions.Count == 0) return true;

        bool allDisabled = true;
        foreach (RestrictionData restrictionData in fixtureInstance.Data.Restrictions)
        {
            if (!restrictionData.Enabled) continue;
            allDisabled = false;

            bool manditoryVsProhibited;
            bool condition = true;
            foreach (Restriction restriction in restrictionData.Restrictions)
            {
                manditoryVsProhibited = restriction.Type == Restriction.RestrictionType.Manditory;
                condition = true;

                if (restriction.HasTileType && condition)
                {
                    foreach (Vector2Int position in restriction.Positions)
                    {
                        Vector3 rotatedPosition = fixtureInstance.RotationMatrix * (Vector2)position;
                        Vector2Int offsetPosition = fixtureInstance.Position + new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
                        FacilityGeneration.TileType tileType = FacilityGeneration.TileType.None;

                        if (facilityFloor.TileMap.InBounds(offsetPosition))
                        {
                            tileType = facilityFloor.TileMap[offsetPosition].Type;
                        }

                        if (tileType == restriction.TileType)
                        {
                            if (!manditoryVsProhibited)
                            {
                                condition = false;
                                break;
                            }
                        }
                        else
                        {
                            if (manditoryVsProhibited)
                            {
                                condition = false;
                                break;
                            }
                        }
                    }
                }

                //if (restriction.HasPathToWalkableTile && condition)
                //{
                //    Vector2Int size = fixtureInstance.ParentCollection.Bounds.upperRight - fixtureInstance.ParentCollection.Bounds.lowerLeft;
                //    Pathfinder2D pathfinder2D = new Pathfinder2D(facilityFloor.Size);
                //    Vector2Int end = fixtureInstance.ParentCollection.connections[0].from;

                //    foreach (Vector2Int position in restriction.Positions)
                //    {
                //        Vector3 rotatedPosition = fixtureInstance.RotationMatrix * (Vector2)position;
                //        Vector2Int offsetPosition = fixtureInstance.Position + new Vector2Int((int)rotatedPosition.x, (int)rotatedPosition.y);
                //        if (!facilityFloor.TileMap.InBounds(offsetPosition))
                //        {
                //            if (manditoryVsProhibited)
                //            {
                //                condition = false;
                //                break;
                //            }
                //        }

                //        List<Vector2Int> path = pathfinder2D.FindPath(position, (Pathfinder2D.Node a, Pathfinder2D.Node b) =>
                //        {
                            
                //        });

                //        if (path.Count == 0)
                //        {
                //            if (manditoryVsProhibited)
                //            {
                //                condition = false;
                //                break;
                //            }
                //        }
                //        else
                //        {
                //            if (!manditoryVsProhibited)
                //            {
                //                condition = false;
                //                break;
                //            }
                //        }
                //    }
                //}

                if (!condition)
                {
                    break;
                }
            }

            if (condition)
            {
                return true;
            }
        }

        if (allDisabled)
        {
            return true;
        }

        return false;
    }

    //public static bool VerifyAllFixtureRestrictions(FacilityFloor facilityFloor, TileCollection tileCollection, FixtureInstance newFixtureInstance, IEnumerable<Vector2Int> newFixturePositions)
    //{
    //    foreach (FixtureInstance fixtureInstance in tileCollection.FixtureInstances)
    //    {
    //        if (!VerifyFixtureRestrictions(facilityFloor, tileCollection, fixtureInstance, newFixtureInstance, newFixturePositions)) return false;
    //    }

    //    return true;
    //}

    public static FixtureInstance PlaceRandomRelationship(FacilityFloor facilityFloor, TileCollection tileCollection, FixtureInstance fixtureInstance)
    {
        if (fixtureInstance.Data.Relationships.Count == 0) return null;

        int randomRelationshipIndex = Random.Range(0, fixtureInstance.Data.Relationships.Count);

        for (int i = 0; i < fixtureInstance.Data.Relationships.Count; i++)
        {
            FixtureRelationshipData relationshipData = fixtureInstance.Data.Relationships[(randomRelationshipIndex + i) % fixtureInstance.Data.Relationships.Count];

            if (!relationshipData.Enabled) continue;

            FixtureData fixtureData = relationshipData.OtherFixture;
            Vector3 originRotatedPosition = fixtureInstance.RotationMatrix * (Vector2)relationshipData.Position;
            Vector2Int position = (new Vector2Int((int)originRotatedPosition.x, (int)originRotatedPosition.y) + fixtureInstance.Position);

            RotationPreset newRotationPreset = (RotationPreset)(((int)relationshipData.RotationPreset + (int)fixtureInstance.RotationPreset) % 4);

            FixtureInstance newFixtureInstance = new FixtureInstance(fixtureData, tileCollection, position, FacilityGenerationManager.GetRotationMatrix(newRotationPreset), newRotationPreset);

            if (TryPlaceFixture(facilityFloor, tileCollection, newFixtureInstance)) return newFixtureInstance;
        }

        return null;
    }

    //private static MultiNodePathfinder2D.EvaluationInfo CalculatePathCost(FacilityFloor facilityFloor, Vector2Int end, MultiNodePathfinder2D.Node from, List<MultiNodePathfinder2D.Node> to)
    //{
    //    MultiNodePathfinder2D.EvaluationInfo evaluationInfo = new MultiNodePathfinder2D.EvaluationInfo();

    //    evaluationInfo.Cost = Mathf.Pow(Vector2Int.Distance(to[0].Position, end), 2);

    //    MultiNodePathfinder2D.Node previous = from.Previous;
    //    if (previous != null)
    //    {
    //        Vector2Int previousDifference = previous.Position - from.Position;
    //        Vector2Int difference = from.Position - to[0].Position;

    //        if (previousDifference != difference)
    //        {
    //            evaluationInfo.Cost += straightnessPenalty;
    //        }
    //    }

    //    evaluationInfo.Traversable = true;
    //    bool reachedEnd = true;
    //    foreach (MultiNodePathfinder2D.Node node in to)
    //    {
    //        if (facilityFloor.TileMap[node.Position].TileCollection != facilityFloor.TileMap[end].TileCollection)
    //        {
    //            reachedEnd = false;
    //            break;
    //        }
    //    }

    //    if (reachedEnd)
    //    {
    //        if (useExactEndPosition)
    //        {
    //            bool success = false;
    //            foreach (MultiNodePathfinder2D.Node node in to)
    //            {
    //                if (node.Position == end)
    //                {
    //                    success = true;
    //                    break;
    //                }
    //            }

    //            if (!success) evaluationInfo.Traversable = false;
    //        }

    //        evaluationInfo.Cost += 5;
    //        return evaluationInfo;
    //    }

    //    FacilityGeneration.Tile tile = facilityFloor.TileMap[to[0].Position];

    //    if (tile.Type == FacilityGeneration.TileType.None)
    //    {
    //        evaluationInfo.Cost += 25;
    //    }
    //    else if (tile.Type == FacilityGeneration.TileType.Hallway)
    //    {
    //        evaluationInfo.Cost += 5;
    //    }
    //    else
    //    {
    //        evaluationInfo.Traversable = false;
    //        return evaluationInfo;
    //    }

    //    for (int i = 1; i < to.Count; i++)
    //    {
    //        if (facilityFloor.TileMap[to[i].Position].Type != tile.Type)
    //        {
    //            //if (tile.Type == FacilityGeneration.TileType.Hallway && facilityFloor.TileMap[to[i].Position].Type == FacilityGeneration.TileType.None) continue;
    //            evaluationInfo.Traversable = false;
    //            return evaluationInfo;
    //        }
    //    }

    //    return evaluationInfo;
    //}
}