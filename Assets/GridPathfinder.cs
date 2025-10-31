using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class GridPathfinder : MonoBehaviour
{
    private globalGrid globalGrid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        globalGrid = GetComponent<globalGrid>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<Vector3> FindPath(Vector3[,] grid, Vector3 start, Vector3 end)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        float[,] distances = new float[rows, cols];
        bool[,] visited = new bool[rows, cols];
        Node[,] nodes = new Node[rows, cols];

        //Start with all distances at infinity
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                distances[i, j] = float.MaxValue;
                nodes[i, j] = new Node(i, j, float.MaxValue);
            }
        }

        SortedSet<Node> openSet = new SortedSet<Node>(new NodeComparer());

        //Finding index pos's
        //Start
        int[] startIndexes = getClosestOnGridPointTo(grid, start); //Give in form [x,y]
        int startX = startIndexes[0];
        int startY = startIndexes[1];

        Debug.Log($"Start location: {grid[startX, startY]}");

        //End
        int[] endIndexes = getClosestOnGridPointTo(grid, end);
        int endX = endIndexes[0];
        int endY = endIndexes[1];

        Debug.Log($"End location: {grid[endX, endY]}");

        //Starting node
        distances[startX, startY] = 0;
        nodes[startX, startY].distance = 0;
        openSet.Add(nodes[startX, startY]);

        while (openSet.Count > 0)
        {
            Node current = openSet.Min;
            openSet.Remove(current);

            int x = current.x;
            int y = current.y;

            //Already visited so skip
            if (visited[x, y]) { continue; }

            visited[x, y] = true;

            //Check if this is goal
            if (x == endX && y == endY)
            {
                //Reconstruct the path
                return ReconstructPath(nodes, startX, startY, endX, endY, grid);
            }

            //Check all neighbors
            foreach (var (nx, ny) in GetIndexNeighbours(x, y, grid))
            {
                if (visited[nx, ny]) { continue; }

                float edgeCost = Vector3.Distance(grid[x, y], grid[nx, ny]);
                float newDistance = distances[x, y] + edgeCost;

                if (newDistance < distances[nx, ny])
                {
                    openSet.Remove(nodes[nx, ny]);

                    distances[nx, ny] = newDistance;
                    nodes[nx, ny].distance = newDistance;
                    nodes[nx, ny].parent = current;

                    openSet.Add(nodes[nx, ny]);
                }
            }
        }

        return null; //No path found!
    }
    
    private List<Vector3> ReconstructPath(Node[,] nodes, int startX, int startY, int endX, int endY, Vector3[,] grid)
    {
        List<Vector3> path = new List<Vector3>();
        Node current = nodes[endX, endY];

        while (current != null)
        {
            path.Add(grid[current.x, current.y]);

            if (current.x == startX && current.y == startY)
                break;

            current = current.parent;
        }

        path.Reverse();
        return path;
    }


    private List<(int, int)> GetIndexNeighbours(int x, int y, Vector3[,] grid)
    {
        List<(int, int)> neighbours = new List<(int, int)>();

        //Only adds orthongonal directions
        if (globalGrid.isInBounds(x - 1, y, grid)) { neighbours.Add((x - 1, y)); }
        if (globalGrid.isInBounds(x + 1, y, grid)) { neighbours.Add((x + 1, y)); }
        if (globalGrid.isInBounds(x, y - 1, grid)) { neighbours.Add((x, y - 1)); }
        if (globalGrid.isInBounds(x, y + 1, grid)) { neighbours.Add((x, y + 1)); }

        return neighbours;
    }

    private class Node
    {
        public int x, y;
        public float distance;
        public Node parent;

        public Node(int x, int y, float distance)
        {
            this.x = x;
            this.y = y;
            this.distance = distance;
        }
    }

    private class NodeComparer : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            int result = x.distance.CompareTo(y.distance);
            if (result == 0)
            {
                result = x.x.CompareTo(x.x);
                if (result == 0)
                {
                    result = y.y.CompareTo(y.y);
                }
            }
            return result;
        }
    }

    //Returns in the form of [x,y]
    private int[] getClosestOnGridPointTo(Vector3[,] grid, Vector3 point)
    {
        // The lazy but computationally intesive way
        int savedX = 0; int savedY = 0; //default to zero zero
        float smallestDistance = float.MaxValue; //if you manage to pick a point bigger than this you have bigger problems

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                Vector3 pos = grid[i, j];

                if (pos == globalGrid.getInvalidVector()) { continue; } //If it's invalid then skip

                float currentDistance = Vector3.Distance(pos, point);

                if (currentDistance < smallestDistance)
                {
                    smallestDistance = currentDistance;
                    savedX = i;
                    savedY = j;
                }
            }
        }
        return new int[] { savedX, savedY };
    }
}
