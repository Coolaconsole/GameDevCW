using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SpawnManager : MonoBehaviour
{
    // map from prefab enemy to their 'spawn cost'
    public List<EntityCostPair> entityCostPairs = new List<EntityCostPair>();

    private List<Vector2Int> spawnPoints = new List<Vector2Int>();

    public void createNewSpawnPoint()
    {
        // select an coordinate on the boundary of the map dimensions
        Vector2Int coord = new Vector2Int();

        Vector2Int mapDimensions = CoordinateManager.Instance.mapDimensions;

        // Warning: may cause infite loop if cant find any free edge tiles
        while (CoordinateManager.Instance.getCoordinateOccupation(coord) != OccupationType.None)
        {
            int edge = Random.Range(0, 4);
            switch (edge)
            {
                case 0:  //  Top edge 
                    coord = new Vector2Int(Random.Range(0, mapDimensions.x), mapDimensions.y - 1);
                    break;
                case 1:  // Bottom edge
                    coord = new Vector2Int(Random.Range(0, mapDimensions.x), 0);
                    break;
                case 2:  // Left edge
                    coord = new Vector2Int(0, Random.Range(0, mapDimensions.y));
                    break;
                case 3:  // Right edge
                    coord = new Vector2Int(mapDimensions.x - 1, Random.Range(0, mapDimensions.y));
                    break;
            }
        }

        spawnPoints.Add(coord);
    }
}

[System.Serializable]
public class EntityCostPair
{
    public GameObject entity;
    public int cost;
}