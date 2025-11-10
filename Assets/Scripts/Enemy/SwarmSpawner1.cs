using UnityEngine;

public class SwarmSpawner : MonoBehaviour
{
    public int swarmSize = 5;
    public GameObject swarmEnemyPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < swarmSize; i++)
        {
            EnemyController script = GetComponent<EnemyController>();
            // Instantiate swarm members around the spawner's position
            Vector3 offset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            GameObject newEnemy = Instantiate(swarmEnemyPrefab, transform.position + offset, Quaternion.identity);
            newEnemy.GetComponent<EnemyController>().path = script.path;
            newEnemy.GetComponent<EnemyController>().pathOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f));

        }
    }
    void Update()
    {
        Destroy(gameObject);
    }
}
