using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class cubeGenerator : MonoBehaviour
{
    [Header("Cube stats")]
    [SerializeField] private GameObject cube;
    [SerializeField] private int numCubes = 10; //number of cubes to randomly spawn
    [SerializeField] private int minSize = 1;
    [SerializeField] private int maxSize = 10;

    [Header("Grid parts")]
    [SerializeField] private globalGrid globalGrid;
    [SerializeField] private GameObject allCubesParent;

    private Vector3[,] grid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Gets the whole grid
        grid = globalGrid.generateFullGrid();

        Debug.Log("Start is running and have generated grid");

        int gridWidth = grid.GetLength(0);
        int gridLength = grid.GetLength(1);

        //Loop through number of cubes needed
        for (int i = 0; i < numCubes; i++)
        {
            int randomX = Random.Range(0, gridWidth);
            int randomY = Random.Range(0, gridLength);

            //Create and scale point at given random spot 
            GameObject newCube = Instantiate(cube, grid[randomX, randomY], Quaternion.identity, allCubesParent.transform);
            newCube.transform.localScale *= Random.Range(minSize, maxSize);
        }

        Physics.SyncTransforms(); //Forces unity to do cube stuff now!

        //Removes the cells where obstacles are
        grid = globalGrid.removeObstaclesFromGrid(grid);

        //Remove obstacles from the scene
        Destroy(allCubesParent);
    }

    void OnDrawGizmosSelected()
    {
        try
        {
            foreach(Vector3 pos in grid)
            {
                    if (pos == globalGrid.getInvalidVector()) { continue; }

                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(pos, new Vector3(1, 1, 1));
                }
        }
        catch (NullReferenceException)
        {
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
