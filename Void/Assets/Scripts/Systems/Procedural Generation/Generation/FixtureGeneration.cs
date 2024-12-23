using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class FixtureGeneration
{
    public static List<Vector2Int> GetFixtureInteriorPositions(FixtureInstance fixtureInstance)
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
        Vector2Int randomPosition = facilityFloor.InteriorTilesPerMapTile * tileCollection.GetRandomMapTilePosition() + new Vector2Int(Random.Range(0, facilityFloor.InteriorTilesPerMapTile), Random.Range(0, facilityFloor.InteriorTilesPerMapTile));
        RotationPreset randomRotationPreset = (RotationPreset)Random.Range(0, 4);
        Matrix4x4 rotationMatrix = FacilityGenerationManager.GetRotationMatrix(randomRotationPreset);

        FixtureInstance fixtureInstance = new FixtureInstance(fixtureData, tileCollection, randomPosition, rotationMatrix, randomRotationPreset);

        if (TryPlaceFixture(facilityFloor, tileCollection, fixtureInstance)) return fixtureInstance;

        return null;
    }

    public static bool TryPlaceFixture(FacilityFloor facilityFloor, TileCollection tileCollection, FixtureInstance fixtureInstance)
    {
        List<Vector2Int> positions = GetFixtureInteriorPositions(fixtureInstance);

        foreach (Vector2Int position in positions)
        {
            if (!facilityFloor.InteriorFloorMap.InBounds(position)) return false;

            // TODO - Scaled position sometimes results in out of bounds error (At max X or Y)
            Vector2Int scaledPosition = position / facilityFloor.InteriorTilesPerMapTile;

            TileCollection otherCollection = facilityFloor.FloorMap[scaledPosition].Collection;
            if (otherCollection != tileCollection) return false;
        }

        if (!InteriorGeneration.CanPlaceInteriorTiles(facilityFloor, positions)) return false;

        //if (!VerifyFixtureRestrictions(facilityFloor, tileCollection, fixtureInstance, positions)) return false;

        InteriorGeneration.PlaceInteriorTiles(facilityFloor, fixtureInstance, InteriorTile.InteriorTileType.Fixture, positions);
        tileCollection.AddFixtureInstance(fixtureInstance);
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

    //// TODO - Implement optimization (SEND POSITIONS TO VERIFY FUNCTION TO CHECK IF THOSE POSITIONS ARE INSIDE OF THE BOUNDS OF THE RESTRICTIONS AND IF SO, IF THEY NEGATE ITS RESTRICTIONS)
    //public static bool VerifyFixtureRestrictions(FacilityFloor facilityFloor, TileCollection tileCollection, FixtureInstance fixtureInstance, IEnumerable<Vector2Int> positions)
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
    //                    InteriorTile.InteriorTileType interiorTileType = InteriorTile.InteriorTileType.Wall;

    //                    if (facilityFloor.InteriorFloorMap.InBounds(offsetPosition))
    //                    {
    //                        interiorTileType = facilityFloor.InteriorFloorMap[offsetPosition].Type;
    //                    }

    //                    if (interiorTileType == restriction.InteriorTileType)
    //                    {
    //                        if (!manditoryVsProhibited)
    //                        {
    //                            condition = false;
    //                            break;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (manditoryVsProhibited)
    //                        {
    //                            condition = false;
    //                            break;
    //                        }
    //                    }
    //                }
    //            }

    //            if (restriction.HasPathToWalkableTile && condition)
    //            {
    //                Vector2Int size = fixtureInstance.ParentCollection.Bounds.upperRight - fixtureInstance.ParentCollection.Bounds.lowerLeft;
    //                Pathfinder2D pathfinder2D = new Pathfinder2D(size * facilityFloor.InteriorTilesPerMapTile);
    //                Vector2Int end = fixtureInstance.ParentCollection.connections[0].from;

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

    //                    List<Vector2Int> path = pathfinder2D.FindPath(position, end, (Pathfinder2D.Node a, Pathfinder2D.Node b) =>
    //                    {
    //                        Pathfinder2D.PathInfo pathInfo = new Pathfinder2D.PathInfo();

    //                        pathInfo.cost = Vector2Int.Distance(b.Position, end);
    //                        pathInfo.traversable = true;

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
}