using UnityEngine;

public class KamikazeEnemy : EnemyController
{
    public override void Update()
    {
        if (GetComponent<HealthController>().currentHealth <= 0)
        {
            if (PathManager.Instance.pathTileMap.ContainsKey(CoordinateManager.Instance.getNearestWorldPosCoordinate(transform.position)))
                PathManager.Instance.pathTileMap[CoordinateManager.Instance.getNearestWorldPosCoordinate(transform.position)].GetComponent<PathTileController>().updateActivity(attackDamage / 40);

            SpawnManager.Instance.decrementNumAliveEnemies();
            AudioManager.Instance.PlaySFX("enemy dies", 0.3f, 0.6f, 1.4f);
            GameObject coin = Instantiate(coinObject, transform.position + Vector3.up, Random.rotation);
            coin.GetComponent<CoinController>().setValue(coinValue);
            Destroy(gameObject);
        }

        targetController.UpdateTarget();
        if (targetController.currentTarget != GetComponent<Projectile>().target)
            GetComponent<Projectile>().target = targetController.currentTarget;

        // otherwise, follow path
        if (isFollowingPath && pathIndex < path.Count)
            followPath();
    }

    private void Start()
    {
        AudioManager.Instance.PlaySFX("laugh", 0.4f, 0.8f, 1.3f);
    }

    public override void OnTriggerEnter(Collider other)
    {
        GameObject proj = other.gameObject;
        Projectile projectileComponent = proj.GetComponent<Projectile>();
        if (projectileComponent != null && projectileComponent.target != null && projectileComponent.target.Equals(gameObject))
        {

            HealthController hc = GetComponent<HealthController>();
            if (proj.CompareTag("Projectile") && hc != null)
            {

                Projectile shot = (Projectile)proj.GetComponent(typeof(Projectile));
                hc.TakeDamage(Mathf.Max(shot.GetDamage() - (int)defence, 1));
            }
            // dont destroy if its the player hitbox
            if (proj.GetComponentInParent<PlayerAttack>() == null)
            {
                Destroy(proj);
            }
        }
    }
}
