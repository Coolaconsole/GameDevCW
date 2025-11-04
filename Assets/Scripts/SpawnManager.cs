using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    // map from prefab enemy to their 'spawn cost'
    public List<EntityCostPair> enemyCostPairs = new List<EntityCostPair>();

    public Vector2Int baseCoord;
    public List<Vector2Int> spawnPoints = new List<Vector2Int>();

    public float waveCooldown = 5.0f;  // might change from cooldown to some player-control like interact with base
    public float timeSinceWaveEnded;
    public int numCurrentWave;
    public bool waveInProgress = false;
    public int waveSpawnBudget;  // potential bug: if cant reach 0 (i.e. no enemy with cost 1), then wave will never end
    public int numAliveEnemies;
    public float spawnCooldown = 1.0f;
    public float timeSinceLastSpawn;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (waveInProgress && waveSpawnBudget > 0)
        {
            if (timeSinceLastSpawn > spawnCooldown)
                spawnEnemy();
            else
                timeSinceLastSpawn += Time.deltaTime;
        }
        else if (!waveInProgress)
        {
            if (timeSinceWaveEnded > waveCooldown)
                beginNewWave();
            else
                timeSinceWaveEnded += Time.deltaTime;
        }
    }


    public void beginNewWave()
    {
        numCurrentWave += 1;
        // recalculate spawn budget via some function:
        waveSpawnBudget = numCurrentWave * 5;

        createNewEnemyPath();

        waveInProgress = true;
    }

    public void spawnEnemy()
    {
        // to do - make more elaborate
        if (enemyCostPairs[0].cost <= waveSpawnBudget)
        {
            Vector2Int randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Count)];
            
            GameObject newEnemy = Instantiate(enemyCostPairs[0].entity, CoordinateManager.Instance.getCoordinateWorldPos(randomSpawn), Quaternion.identity);
            newEnemy.GetComponent<EnemyController>().path = PathManager.Instance.getAPath(randomSpawn);
            
            waveSpawnBudget -= enemyCostPairs[0].cost;
            timeSinceLastSpawn = 0;
        }
    }

    public Vector2Int createNewSpawnPoint()
    {
        // select an coordinate on the boundary of the map dimensions
        Vector2Int coord = new Vector2Int();
        Vector2Int mapDimensions = CoordinateManager.Instance.mapDimensions;
        bool doneOnce = false;  // fix to make sure code runs at least once

        // Warning: may cause infite loop if cant find any free edge tiles
        while (!doneOnce || ( CoordinateManager.Instance.getCoordinateOccupation(coord) != OccupationType.None))
        {
            doneOnce = true;
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
        return coord;
    }

    public void createNewEnemyPath()
    {
        Vector2Int newSpawnPoint = createNewSpawnPoint();
        // generate new path from new spawn to base
        PathManager.Instance.generateAPath(newSpawnPoint, baseCoord);
    }

    // called when an enemie dies to keep track of when a wave is 
    public void decrementNumAliveEnemies()
    {
        if (waveInProgress && numAliveEnemies > 0)
        {
            numAliveEnemies -= 1;
            if (numAliveEnemies == 0)
                waveInProgress = false;
        }
    }
}

[System.Serializable]
public class EntityCostPair
{
    public GameObject entity;
    public int cost;
}
