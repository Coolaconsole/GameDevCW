using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;

public enum OccupationType{
    None,  // not stored in occupiedCoordinates, just for functions return value completeness
    Path,
    Tower,
    Base
}

public class CoordinateManager : MonoBehaviour
{
    public static CoordinateManager Instance { get; private set; }

    public Vector2Int mapDimensions = new Vector2Int(10, 10);  // exclusive
    public float cellSize = 2.0f;
    public Vector3 gridOrigin = Vector3.zero;
    public Dictionary<Vector2Int, OccupationType> occupiedCoordinates = new Dictionary<Vector2Int, OccupationType>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Vector3 getCoordinateWorldPos(Vector2Int coord)
    {
        Vector3 bottomLeft = gridOrigin - new Vector3(mapDimensions.x, gridOrigin.z, mapDimensions.y) * 0.5f * cellSize;

        return bottomLeft + new Vector3((coord.x + 0.5f) * cellSize, 0, (coord.y + 0.5f) * cellSize);
    }

    public Vector2Int getNearestWorldPosCoordinate(Vector3 pos)
    {
        Vector3 bottomLeft = gridOrigin - new Vector3(mapDimensions.x, 0, mapDimensions.y) * 0.5f * cellSize;

        float localX = (pos.x - bottomLeft.x) / cellSize;
        float localY = (pos.z - bottomLeft.z) / cellSize;

        int coordX = Mathf.Clamp(Mathf.FloorToInt(localX), 0, mapDimensions.x - 1);
        int coordY = Mathf.Clamp(Mathf.FloorToInt(localY), 0, mapDimensions.y - 1);

        return new Vector2Int(coordX, coordY);
    }

    public OccupationType getCoordinateOccupation(Vector2Int coord)
    {
        if (!occupiedCoordinates.ContainsKey(coord)) return OccupationType.None;

        return occupiedCoordinates[coord];
    }

    public void occupyCoordinate(Vector2Int coord, OccupationType type)
    {
        if (occupiedCoordinates.ContainsKey(coord)) return;  // prevent duplicates

        occupiedCoordinates[coord] = type;
    }

    public void freeCoordinate(Vector2Int coord)
    {
        occupiedCoordinates.Remove(coord);
    }

    public bool isAdjacentTo(Vector2Int coord, OccupationType type)
    {
        // explore neighbours
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (Math.Abs(dx) + Math.Abs(dy) != 1) continue; // skip diagonals and self
                int nx = coord.x + dx;
                int ny = coord.y + dy;
                if (nx < 0 || nx >= mapDimensions.x || ny < 0 || ny >= mapDimensions.y) continue;

                Vector2Int neighbor = new Vector2Int(nx, ny);

                if (getCoordinateOccupation(neighbor) == type)
                    return true;
            }
        }

        return false;
    }
}
