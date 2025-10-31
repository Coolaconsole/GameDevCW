using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{

    [Header("Other Components")]
    [SerializeField] private cubeGenerator gridGenerator;
    [SerializeField] private globalGrid globalGrid;
    [SerializeField] private GridPathfinder gridPathfinder;
    [SerializeField] private Transform target;

    private Vector3[,] validGrid;
    void Start()
    {
        Debug.Log("I have started running! (Enemy)");
        validGrid = gridGenerator.generateObstacleGrid();

        List<Vector3> path = gridPathfinder.FindPath(validGrid, transform.position, target.position);

        Debug.Log(path);

        foreach (Vector3 pos in path)
        {
            Debug.Log("Point: " + pos);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
