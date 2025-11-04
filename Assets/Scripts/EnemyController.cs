using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

// most basic enemy controller with no special abilities/behaviours/preferences
public class EnemyController : MonoBehaviour
{
    public List<Vector2Int> path;
    public int pathIndex;
    public Vector3 pathOffset;  // for more natural-looking enemy behaviour
    public bool isFollowingPath = true;  // true when enemy is moving allong path

    public bool willTargetPlayer;
    public GameObject currentTarget;

    public float moveSpeed;
    public float attackRange;
    public float attackDamage;
    public float attackCooldown;
    private float lastAttackTime;
    

    private void Update()
    {
        // check if there is a new target

        // if target is not null, stop following path and attack

        // otherwise, follow path
        if (isFollowingPath && pathIndex < path.Count)
            followPath();
    }

    private void findTarget()
    {

    }

    private void followPath()
    {
        if (path.Count == 0) return;

        Vector3 targetPos = CoordinateManager.Instance.getCoordinateWorldPos(path[pathIndex]) + pathOffset;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.05)
        {
            pathIndex += 1;
            // reached final position in path
            if (pathIndex == path.Count)
                isFollowingPath = false;
        }
    }
}
