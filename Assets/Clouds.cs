using UnityEngine;

public class CloudManager : MonoBehaviour
{
    [SerializeField] private float minSpawnX;
    [SerializeField] private float maxSpawnX;
    [SerializeField] private float spawnRate;
    [SerializeField] private GameObject cloud;
    [SerializeField] private Transform spawnPos;
    private float spawnTimer; 

    private bool waitingToSpawn = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (waitingToSpawn)
        {
            //Reset timer
            spawnTimer = spawnRate;

            // Put a random spot
            Vector3 cloudSpawnPos = spawnPos.position;
            cloudSpawnPos.x += Random.Range(minSpawnX, maxSpawnX);

            Instantiate(cloud, cloudSpawnPos, Quaternion.identity);

            //No longer waiting to spawn
            waitingToSpawn = false;
        }
        //Decrease the timer
        spawnTimer -= Time.deltaTime;


        //If the timer is zero then we can spawn next frame
        if (spawnTimer <= 0) { waitingToSpawn = true; }
    }
}
