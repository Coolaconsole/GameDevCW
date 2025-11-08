using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    // map from prefab enemy to their 'spawn cost'
    public List<EntityCostInfo> enemyCostInfos = new List<EntityCostInfo>();
    private List<EntityCostInfo> currentEnemies = new List<EntityCostInfo>();
    private List<float> currentEnemyWeights = new List<float>();
    private float totalWeight;

    public GameObject waveInfoUI;
    public GameObject inventoryUI;

    public Vector2Int baseCoord;
    public List<Vector2Int> spawnPoints = new List<Vector2Int>();

    public float waveCooldown = 3.0f;  // might change from cooldown to some player-control like interact with base
    public float timeSinceWaveEnded;
    public int numCurrentWave;
    public bool waveInProgress = false;
    public int waveSpawnBudget;  // potential bug: if cant reach 0 (i.e. no enemy with cost 1), then wave will never end
    public int numAliveEnemies;
    public float spawnCooldown = 1.0f;
    public float timeSinceLastSpawn;
    private int totalCost;

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
            if (numCurrentWave != 0)
                waveInfoUI.GetComponent<TextMeshProUGUI>().text = "Wave " + numCurrentWave.ToString() + " Complete! Next wave in " + (waveCooldown-timeSinceWaveEnded).ToString("F1") + "s";
            if (timeSinceWaveEnded > waveCooldown)
                beginNewWave();
            else
                timeSinceWaveEnded += Time.deltaTime;
        }
    }


    public void beginNewWave()
    {
        numAliveEnemies = FindObjectsOfType<EnemyController>().Length;

        numCurrentWave += 1;
        // recalculate spawn budget via some function:
        waveSpawnBudget = numCurrentWave * 5;
        waveInfoUI.GetComponent<TextMeshProUGUI>().text = "Wave " + numCurrentWave.ToString() + ExtraWaveInfo();
        createNewEnemyPath();

        waveInProgress = true;

        currentEnemies = new List<EntityCostInfo>();
        totalCost = 0;
        foreach (var eci in enemyCostInfos)
        {
            if (eci.waveUnlocked <= numCurrentWave)
            {
                currentEnemies.Add(eci);
                totalCost += eci.cost;
            }
        }
        currentEnemyWeights = new List<float>();
        float weight = 0f;
        foreach (var eci in currentEnemies)
        {
            weight += totalCost - eci.cost;
            currentEnemyWeights.Add(totalCost - eci.cost);  // higher weight to lower cost enemies
        
        }
        totalWeight = weight;
        inventoryUI.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);  // show inventory UI
    }

    public void spawnEnemy()
    {
        // to do - make more elaborate
        EntityCostInfo enemyToSpawn = GetWeightedRandomEnemy(currentEnemies, currentEnemyWeights);

        if (enemyToSpawn.cost <= waveSpawnBudget)
        {
            Vector2Int randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Count)];

            //GameObject newEnemy = Instantiate(enemyCostInfos[0].entity, CoordinateManager.Instance.getCoordinateWorldPos(randomSpawn), Quaternion.identity);
            GameObject newEnemy = Instantiate(enemyToSpawn.entity, CoordinateManager.Instance.getCoordinateWorldPos(randomSpawn), Quaternion.identity);
            newEnemy.GetComponent<EnemyController>().path = PathManager.Instance.getAPath(randomSpawn);
            newEnemy.GetComponent<EnemyController>().pathOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));

            //waveSpawnBudget -= enemyCostInfos[0].cost;
            waveSpawnBudget -= enemyToSpawn.cost;
            timeSinceLastSpawn = 0;
            if(newEnemy.tag != "Enemy")
            {
                numAliveEnemies += 5;  // for swarm spawners
            } else
                numAliveEnemies += 1;
        } else if (waveSpawnBudget > 0)
        {
            spawnEnemy();  // try again
        }
    }
    
    EntityCostInfo GetWeightedRandomEnemy(List<EntityCostInfo> enemies, List<float> weights)
    {
        float r = Random.value * totalWeight;
        float cumulativeWeight = 0;
        

        for (int i = 0; i < enemies.Count; i++)
        {
            cumulativeWeight += weights[i];
            if (cumulativeWeight >= r)
            {
                return enemies[i];
            }
        }

        return enemies[enemies.Count - 1];
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
                    coord = new Vector2Int(Random.Range(0, mapDimensions.x / 2), mapDimensions.y - 1);
                    // coord = new Vector2Int(Random.Range(0, mapDimensions.x), mapDimensions.y - 1);
                    
                    break;
                case 1:  // Bottom edge
                    // coord = new Vector2Int(Random.Range(0, mapDimensions.x / 2), 0);
                    coord = new Vector2Int(Random.Range(0, mapDimensions.x), 0);
                    break;
                case 2:  // Left edge
                    coord = new Vector2Int(0, Random.Range(0, mapDimensions.y));
                    break;
                // case 3:  // Right edge
                //     coord = new Vector2Int(mapDimensions.x - 1, Random.Range(0, mapDimensions.y));
                //     break;
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
            if (numAliveEnemies == 0 && waveSpawnBudget == 0)
            {
                waveInProgress = false;
                timeSinceWaveEnded = 0;
                inventoryUI.transform.localScale = new Vector3(1f, 1f, 1f);
                waveInfoUI.GetComponent<TextMeshProUGUI>().text = "Wave " + numCurrentWave.ToString() + " Complete! Next wave in " + waveCooldown.ToString("F1") + "s";
            }
        }
    }

    void SetupPresetEnemies()
    {
        /*
        Wave 1: 1 cost 1 enemy
        Wave 2: 3 cost 1 enemies
        Wave 3: Not preset
        Wave 4: Not preset
        Wave 7: 20 cost 1 enemies, 5 cost 3 enemies
        */
        
    }
    
    // EntityCostInfo GetPresetEnemy()
    // {
        
    //     return enemyCostInfos[index];
    // }

    string ExtraWaveInfo()
    {
        switch(numCurrentWave)
        {
            case 1:
                return "\nMove with WASD. Shoot enemies with _____.";
            case 3:
                return "\nTry moving your tower with SPACE.";
            case 4:
                return " - New Enemy Unlocked!";
            case 7:
                return " - New Enemy Unlocked!";
            case 10:
                return " - Boss Wave!";
            default:
                break;
        }
        return "";
    }
}

[System.Serializable]
public class EntityCostInfo
{
    public GameObject entity;
    public int cost;
    public int waveUnlocked;
}
    