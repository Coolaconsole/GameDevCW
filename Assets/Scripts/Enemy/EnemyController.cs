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
    
    public TargetController targetController;

    public GameObject projectile;
    public float moveSpeed;
    public float attackRange;
    public float attackDamage;
    public float attackCooldown;
    private float lastAttackTime;
    public GameObject coinObject;
    public int coinValue;

    private void Start()
    {
        //GetComponentInChildren<SphereCollider>().radius = attackRange;
        targetController = GetComponentInChildren<TargetController>();
    }

    private void Update()
    {
        // check if there is a new target
        targetController.UpdateTarget();

        // if target is not null, stop following path and attack
        if (targetController.currentTarget != null)
            handleAttack();

        // otherwise, follow path
        else if (isFollowingPath && pathIndex < path.Count)
            followPath();
    }

    private void handleAttack()
    {
        if (lastAttackTime >= attackCooldown)
        {
            Projectile proj = Instantiate(projectile, transform.position + new Vector3(0, 1, 0), Quaternion.identity).GetComponent<Projectile>();
            proj.SetDamage((int)attackDamage);
            proj.SetTarget(targetController.currentTarget);
            lastAttackTime = 0f;
        }
        else
            lastAttackTime += Time.deltaTime;
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

    private void OnDestroy()
    {
        if (PathManager.Instance.pathTileMap.ContainsKey(CoordinateManager.Instance.getNearestWorldPosCoordinate(transform.position)))
            PathManager.Instance.pathTileMap[CoordinateManager.Instance.getNearestWorldPosCoordinate(transform.position)].GetComponent<PathTileController>().updateActivity(attackDamage / 40);

        SpawnManager.Instance.decrementNumAliveEnemies();

        GameObject coin = Instantiate(coinObject, transform.position + Vector3.up, Random.rotation);
        coin.GetComponent<CoinController>().value = coinValue;
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject proj = other.gameObject;
        Projectile projectileComponent = proj.GetComponent<Projectile>();
        if (projectileComponent == null || projectileComponent.target == null || !projectileComponent.target.Equals(gameObject))
            return;

        HealthController hc = GetComponent<HealthController>();
        if (proj.CompareTag("Projectile") && hc != null)
        {
            
            Projectile shot = (Projectile)proj.GetComponent(typeof(Projectile));
            hc.TakeDamage(shot.GetDamage());

            Destroy(gameObject);
        }
    }
}
