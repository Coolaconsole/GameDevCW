using System;
using UnityEngine;

public class globalGrid : MonoBehaviour
{
    [Header("Grid Perams")]
    [SerializeField] private int length;
    [SerializeField] private int width;
    [SerializeField] private int cellSize;

    [Header("Object Detection")]
    [SerializeField] private float objectDetectSphereRadius;
    [SerializeField] private int invalidValue = -200; //Cannot set Vector3 to null, so setting the x,y,z to this is what we're calling "null"
    [SerializeField] private LayerMask obstacles;

    public Vector3[,] grid;

    // Make the global grid
    private void Start()
    {
        
    }


    void OnDrawGizmosSelected()
    {
        Vector3[,] grid = removeObstaclesFromGrid(generateFullGrid());
        foreach(Vector3 pos in grid)
        {
            if (pos == getInvalidVector()) { continue; }

            Gizmos.color = Color.red;
            Gizmos.DrawCube(pos, new Vector3(cellSize / 2, cellSize / 2, cellSize / 2));
        }
    }

    public Vector3[,] generateFullGrid()
    {
        Vector3[,] grid = new Vector3[(width / cellSize), (length / cellSize)];
        //Assumes that Global Grid object is placed at the top left corner
        int xCounter = 0;
        for (int x = 0; x < width; x = x + cellSize)
        {
            int yCounter = 0;
            for (int y = 0; y < length; y = y + cellSize)
            {
                grid[xCounter, yCounter] = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + y);
                yCounter++;
            }
            xCounter++;
        }

        return grid;
    }

    public Vector3[,] removeObstaclesFromGrid(Vector3[,] inputGrid)
    {
        int xSize = inputGrid.GetLength(0);
        int ySize = inputGrid.GetLength(1);
        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize; j++)
            {
                if (Physics.CheckSphere(inputGrid[i, j], objectDetectSphereRadius, obstacles)) //Checks only objects in the "obstacle" layer
                {
                    inputGrid[i, j] = new Vector3(invalidValue, invalidValue, invalidValue);
                }
            }
        }
        return inputGrid;
    }

    public Vector3 getInvalidVector()
    {
        return new Vector3(invalidValue, invalidValue, invalidValue);
    }
    
    public bool isInBounds(int xIndex, int yIndex, Vector3[,] grid)
    {
        if (xIndex < 0 || xIndex >= grid.GetLength(0) || yIndex < 0 || yIndex >= grid.GetLength(1))
        {
            return false;
        }
        if (grid[xIndex, yIndex] == getInvalidVector()) { return false; }
        
        return true;
    }
}
