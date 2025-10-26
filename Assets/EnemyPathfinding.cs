using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    //Dijkstra's just for simplicity
    [Header("Stats")]
    [SerializeField] private int cellsAheadVision; // Number of cells checked per move

    [Header("Other Components")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3[,] validGrid;

    //Private states
    private bool lookingForPath = true; //If it is false assume that the enemy is moving

    void Start()
    {
        FindPath();
    }

    void Update()
    {

    }

    private void FindPath()
    {
        if (!lookingForPath) { return; }


    }
    
    private Vector3 getClosestCellTo(Vector3 pos)
    {
        float minimumX = pos.x - validGrid[0,0].x; //Defualt is first gridPos
        float minimumY = pos.z - validGrid[0, 0].z; //Ditto

        float savedX = 0;
        float savedY = 0;
        foreach (Vector3 gridPos in validGrid)
        {
            //if posistion reached is just equal to current gridPos then found!
            if (gridPos == new Vector3(pos.x, 0f, pos.y))
            {
                return gridPos;
            }

            float currentX = pos.x - gridPos.x;
            float currentY = pos.z - gridPos.z;

            if (currentX < minimumX) { minimumX = currentX; savedX = gridPos.x; }
            else if (currentY < minimumY) { minimumY = currentY; savedY = gridPos.z; }
        }
        return new Vector3(savedX, 0f, savedY);
    }
}
