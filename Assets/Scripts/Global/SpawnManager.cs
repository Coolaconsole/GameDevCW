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
    public EntityCostInfo bossEnemyCostInfo;
    private List<EntityCostInfo> currentEnemies = new List<EntityCostInfo>();
    public List<float> currentEnemyWeights = new List<float>();
    public float totalWeight;

    public GameObject waveInfoUI;
    public GameObject inventoryUI;

    public Vector2Int baseCoord;
    public List<Vector2Int> spawnPoints = new List<Vector2Int>();

    public float waveCooldown = 10.0f;  // might change from cooldown to some player-control like interact with base
    public float timeSinceWaveEnded;
    public int numCurrentWave;
    public bool waveInProgress = false;
    public int waveSpawnBudget;  // potential bug: if cant reach 0 (i.e. no enemy with cost 1), then wave will never end
    public int numAliveEnemies;
    public float spawnCooldown = 1.5f;
    public float timeSinceLastSpawn;
    private int totalCost;
    private int maxWaveSpawnBudget;

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
            {
                string end = (waveCooldown - timeSinceWaveEnded).ToString("F1");
                if (float.Parse(end) <= 0.0f)
                    end = "soon";
                else
                    end = "in " + end + "s";
                waveInfoUI.GetComponent<TextMeshProUGUI>().text = "Wave " + numCurrentWave.ToString() + " Complete! Next wave " + end;
            }
            if (timeSinceWaveEnded > waveCooldown && !TutorialManager.Instance.HasUnclosedPrompts())
                beginNewWave();
            else
                timeSinceWaveEnded += Time.deltaTime;
        }
    }

    public void beginNewWave()
    {
        AudioManager.Instance.PlaySFX("wave", 0.4f, 1.2f, 1.2f);

        numAliveEnemies = FindObjectsOfType<EnemyController>().Length;

        numCurrentWave += 1;
        // recalculate spawn budget via some function:
        if (numCurrentWave <= 3) {waveSpawnBudget = numCurrentWave * 2;}
        else {
            for (int i = numCurrentWave; i > 0; i--)
            {
                if (i > 10)
                {
                    waveSpawnBudget += 10;

                    spawnCooldown -= 0.0001f * (numCurrentWave - 10);
                    if (spawnCooldown < 0.5f)
                        spawnCooldown = 0.5f;
                }
                else
                {
                    waveSpawnBudget += i; // triangular number
                    spawnCooldown -= 0.05f;
                }
            }
        }
        waveInfoUI.GetComponent<TextMeshProUGUI>().text = "Wave " + numCurrentWave.ToString() + ExtraWaveInfo();

        if (new TriangleNumber().Check(numCurrentWave-1)) { createNewEnemyPath(); }

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

        maxWaveSpawnBudget = waveSpawnBudget;

        TutorialPrompt(true);
    }

    public void spawnEnemy()
    {
        // to do - make more elaborate
        EntityCostInfo enemyToSpawn = GetWeightedRandomEnemy(currentEnemies, currentEnemyWeights);
        // If wave multiple of 10, spawn boss halfway through the wave, but each third of the wave in wave 20, each quarter of the wave wave 30
        if (numCurrentWave % 5 == 0)
        {
            int threshold = 0;
            if (numCurrentWave == 10 || numCurrentWave == 15)
                threshold = maxWaveSpawnBudget / 2;
            else if (numCurrentWave == 20)
                threshold = maxWaveSpawnBudget * 2 / 3;
            else if (numCurrentWave >= 30)
                threshold = maxWaveSpawnBudget * 3 / 4;

            if (waveSpawnBudget <= threshold)
            {
                enemyToSpawn = bossEnemyCostInfo;
                maxWaveSpawnBudget -= 55;
            }
        }
        if (enemyToSpawn.cost <= waveSpawnBudget)
        {
            Vector2Int randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Count)];

            //GameObject newEnemy = Instantiate(enemyCostInfos[0].entity, CoordinateManager.Instance.getCoordinateWorldPos(randomSpawn), Quaternion.identity);
            GameObject newEnemy = Instantiate(enemyToSpawn.entity, CoordinateManager.Instance.getCoordinateWorldPos(randomSpawn), Quaternion.identity);
            newEnemy.GetComponent<EnemyController>().path = PathManager.Instance.getAPath(randomSpawn);
            newEnemy.GetComponent<EnemyController>().pathOffset = new Vector3(Random.Range(-0.5f, 0.5f), enemyToSpawn.height, Random.Range(-0.5f, 0.5f));

            if(numCurrentWave < 4)
                newEnemy.GetComponent<EnemyController>().moveSpeed -= 1;

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
                    coord = new Vector2Int(Random.Range(0, mapDimensions.x / 3), mapDimensions.y - 1);
                    // coord = new Vector2Int(Random.Range(0, mapDimensions.x), mapDimensions.y - 1);
                    
                    break;
                case 1:  // Bottom edge
                    coord = new Vector2Int(Random.Range(0, mapDimensions.x / 3), 0);
                    // coord = new Vector2Int(Random.Range(0, mapDimensions.x), 0);
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
                PlayerHotBarManager.Instance.SpendCoin(numCurrentWave * -1);  // reward player with coins
                inventoryUI.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                waveInfoUI.GetComponent<TextMeshProUGUI>().text = "Wave " + numCurrentWave.ToString() + " Complete! Next wave in " + waveCooldown.ToString("F1") + "s";
                TutorialPrompt(false);
            }
        }
    }

    string ExtraWaveInfo()
    {
        switch (numCurrentWave)
        {
            case 1:
                return "\nMove with <b>WASD</b> - Attack enemies with <b>SPACE</b>.";
            case 2:
                return "\nSelect your towers with <b>1-4</b> - Place with <b>SPACE</b>.";
            case 3:
                return "\nTry picking your tower up with <b>E</b>, place it back down with <b>Q</b>.";
            case 5:
                return " - New <color=red>Enemy</color> Encountered\nYour attack will be the most effective here.";
            case 4:
                return "\nYour towers heal some of the damage they take at the end of each round.";
            case 8:
                return " - New <color=red>Enemy</color> Encountered\nThey're slow, but tanky!";
            case 12:
                return " - New <color=red>Enemy</color> Encountered\nWatch the skies!";
            case 17:
                return " - New <color=red>Enemy</color> Encountered\nDon't let them reach the castle!";
            default:
                if (numCurrentWave % 5 == 0 && numCurrentWave >= 10)
                    return " - Boss Wave!";
                else if (new TriangleNumber().Check(numCurrentWave-1))
                    return " - New <color=yellow>Enemy Path</color>";
                break;
        }
        return "";
    }
    
    void TutorialPrompt(bool startOfWave)
    {
        switch (numCurrentWave)
        {
            case 1:
                if (startOfWave)
                {
                    TutorialManager.Instance.QueuePrompt("wave1");   
                }
                else
                {
                    TutorialManager.Instance.QueuePrompt("pickupMoney");
                    TutorialManager.Instance.QueuePrompt("firstTower");
                    TutorialManager.Instance.QueuePrompt("backToAttack");
                }
                break;
            case 2:
                if (startOfWave)
                    TutorialManager.Instance.QueuePrompt("towerExplanation");
                break;
            case 3:
                if (startOfWave){
                    TutorialManager.Instance.QueuePrompt("newPath");
                    TutorialManager.Instance.QueuePrompt("moveTower");
                }else
                    TutorialManager.Instance.QueuePrompt("healing");
                    break;
            case 5:
                if (startOfWave)
                    TutorialManager.Instance.QueuePrompt("newEnemy");
                else
                    TutorialManager.Instance.QueuePrompt("pathColour");
                    break;
            case 6:
                if (!startOfWave)
                    TutorialManager.Instance.QueuePrompt("anotherPath");
                break;
            case 7:
                if (!startOfWave)
                    TutorialManager.Instance.QueuePrompt("checkIn");
                break;
            case 8:
                if (startOfWave)
                    TutorialManager.Instance.QueuePrompt("toughEnemy");
                break;
            case 9:
                if (!startOfWave)
                    {TutorialManager.Instance.QueuePrompt("bossEnemy"); 
                    TutorialManager.Instance.QueuePrompt("bossPrep");}
                break;
            case 12:
                if (startOfWave)
                    TutorialManager.Instance.QueuePrompt("flyingEnemy");
                break;
            case 11:
                if (!startOfWave)
                    AudioManager.Instance.PlayMusic("2", 0.7f);
                break;
            case 14:
                if (!startOfWave)
                    TutorialManager.Instance.QueuePrompt("bossLevels");
                break;
            case 17:
                if (startOfWave)
                    TutorialManager.Instance.QueuePrompt("kamikaze");
                break;
            case 21:
                if (!startOfWave)
                    AudioManager.Instance.PlayMusic("3", 0.9f);
                break;
            case 25:
                if (!startOfWave)
                    TutorialManager.Instance.QueuePrompt("success");
                break;
        }
    }
}

[System.Serializable]
public class EntityCostInfo
{
    public GameObject entity;
    public int cost;
    public int waveUnlocked;
    public float height;
}
    
public class TriangleNumber
{
    public bool Check(int n)
    {
        if (n == 0) { return true; }
        if (n == 1) { return false; }
        if (n == 2) { return true; }
        if (n == 3) { return false; }
        int x = 8 * n + 1;
        int s = (int)Mathf.Sqrt(x);
        return s * s == x;
    }
}