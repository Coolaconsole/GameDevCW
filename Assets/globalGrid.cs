using Unity.VisualScripting;
using UnityEngine;

public class globalGrid : MonoBehaviour
{
    [Header("Grid Perams")]
    [SerializeField] private int length;
    [SerializeField] private int width;
    [SerializeField] private int cellSize;

    public Vector3[,] grid;

    // Make the global grid
    private void Start()
    {
        grid = new Vector3[width, length];

        Debug.Log("Start running");
        //Assumes that Global Grid object is placed at the top left corner
        for (int x = 0; x < width; x = x + cellSize)
        {
            for (int y = 0; y < length; y = y + cellSize)
            {
                grid[x, y] = new Vector3(transform.position.x + x, 0f,transform.position.z + y);
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        for (int x = 0; x < width; x = x + cellSize)
        {
            for (int y = 0; y < length; y = y + cellSize)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(new Vector3(transform.position.x + x, 0f,transform.position.z + y), new Vector3(cellSize/2, cellSize/2, cellSize/2));
            }
        }
    }
}
