using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }

    public List<OccupationType> pathCompatibleTypes = new List<OccupationType> { OccupationType.Path, OccupationType.None };

    private List<List<Vector2Int>> paths = new List<List<Vector2Int>>();

    public GameObject dummy1;
    public GameObject dummy2;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // test path generation pattern
        paths.Add(generateAPath(new Vector2Int(0, 0), new Vector2Int(23, 12)));
        foreach (Vector2Int coord in paths[0])
        {
            Instantiate(dummy1, CoordinateManager.Instance.getCoordinateWorldPos(coord), Quaternion.identity);
        }
        splitPathAt(paths[0][6]);
        foreach (Vector2Int coord in paths[1])
        {
            Instantiate(dummy2, CoordinateManager.Instance.getCoordinateWorldPos(coord), Quaternion.identity);
        }
        splitPathAt(paths[0][3]);
        foreach (Vector2Int coord in paths[2])
        {
            Instantiate(dummy2, CoordinateManager.Instance.getCoordinateWorldPos(coord), Quaternion.identity);
        }
    }

    List<Vector2Int> generateAPath(Vector2Int start, Vector2Int end)
    {
        int width = CoordinateManager.Instance.mapDimensions.x;
        int height = CoordinateManager.Instance.mapDimensions.y;

        // generate weights for each coord using perlin noise
        float offsetX = UnityEngine.Random.Range(0, 10000);
        float offsetY = UnityEngine.Random.Range(0, 10000);
        float scale = 1.0f / UnityEngine.Random.Range(1, 8);
        float[,] cost = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
                // todo - make it so adjacent paths are expensive to encourange natural spacing out;

                cost[x, y] = 1.0f + Mathf.PerlinNoise((x + offsetX) * scale, (y + offsetY)) * 3.0f +
                    (CoordinateManager.Instance.getCoordinateOccupation(new Vector2Int(x, y)) == OccupationType.Path ? 4f : 0f) +  // added cost on crossing paths
                    (CoordinateManager.Instance.isAdjacentTo(new Vector2Int(x, y), OccupationType.Path) ? 10f : 0f)
                    ;  
        }

        // a* serach using cost from start to end
        List<Vector2Int> open = new List<Vector2Int>();
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();  // from start to node n
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();  // total from start to n plus n to end (heuristic)

        open.Add(start);
        gScore[start] = 0f;
        fScore[start] = Vector2Int.Distance(start, end);

        // shoudn't ever cause infinte loop, all weights are finite
        while (open.Count > 0)
        {
            // get coordinate with lowest fCost (
            Vector2Int current = open[0];
            float bestF = fScore[current];
            for (int i = 1; i < open.Count; i++)
            {
                float f = fScore[open[i]];
                if (f < bestF)
                {
                    bestF = f;
                    current = open[i];
                }
            }

            if (current == end)
                break; // reached destination

            open.Remove(current);
            closed.Add(current);

            // explore neighbours
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (Math.Abs(dx) + Math.Abs(dy) != 1) continue; // skip diagonals and self
                    int nx = current.x + dx;
                    int ny = current.y + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                    Vector2Int neighbor = new Vector2Int(nx, ny);
                    // only use coords that can be used for a new path
                    if (!pathCompatibleTypes.Contains(CoordinateManager.Instance.getCoordinateOccupation(neighbor)))
                        closed.Add(neighbor);

                    if (closed.Contains(neighbor)) continue;

                    float moveCost = cost[nx, ny];
                    float tentativeG = gScore[current] + moveCost;

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])  // if this is a new shortest known path to current neighbour
                    {
                        parent[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Vector2Int.Distance(neighbor, end);

                        if (!open.Contains(neighbor))
                            open.Add(neighbor);
                    }
                }
            }
        }

        // build path from a* data structures
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int step = end;
        path.Add(step);
        while (parent.ContainsKey(step))
        {
            step = parent[step];
            path.Add(step);

            CoordinateManager.Instance.occupyCoordinate(step, OccupationType.Path);  // update teh coordinate status
        }
        path.Reverse();

        return path;
    }

    List<Vector2Int> getAPath()
    {
        return paths[UnityEngine.Random.Range(0, paths.Count)];
    }

    void splitPathAt(Vector2Int coord)
    {
        // select path that uses the given coord
        List<Vector2Int> pathToSplit = new List<Vector2Int>();
        foreach (List<Vector2Int> path in paths)
        {
            if (path.Contains(coord))
            {
                pathToSplit = path;
                break;
            }
        }
        if (!pathToSplit.Contains(coord)) return;

        List<Vector2Int> newPath = pathToSplit.TakeWhile(n => n != coord).ToList();
        newPath.AddRange(generateAPath(coord, pathToSplit.Last()));

        paths.Add(newPath);
    }
}
