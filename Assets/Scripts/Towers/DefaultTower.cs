using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefaultTower : MonoBehaviour
{
    protected TargetController targetController;

    [Header("Stats")]

    public int damage = 10;
    public float shootCooldown = 1f;
    protected float timeshootCooldown = 0f;
    [SerializeField] private int cost;
    public int baseCostIncrease = 5;
    
    [Header("Other Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private GameObject muzzleSmoke;
    public GameObject projectile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetController = GetComponentInChildren<TargetController>();
    }

    // Update is called once per frame
    void Update()
    {
        targetController.UpdateTarget();

        timeshootCooldown += Time.deltaTime;
        if (timeshootCooldown >= shootCooldown)
        {
            if (targetController.currentTarget != null)
            {
                //Shoot!
                animator.SetTrigger("Shoot");
                Instantiate(muzzleFlash, muzzle.position, muzzle.rotation);
                Instantiate(muzzleSmoke, muzzle.position, muzzle.rotation);
                
                
                Projectile proj = Instantiate(projectile, muzzle.position, Quaternion.identity).GetComponent<Projectile>();
                proj.SetDamage(damage);
                proj.SetTarget(targetController.currentTarget);
                timeshootCooldown = 0f;
            }
            
        }
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject proj = other.gameObject;
        Projectile projectileComponent = proj.GetComponent<Projectile>();
        if (projectileComponent == null || projectileComponent.target == null || !projectileComponent.target.Equals(gameObject))
            return;
        
        HealthController hc = GetComponent<HealthController>();
        if (hc != null)
        {
            Projectile shot = (Projectile)proj.GetComponent(typeof(Projectile));
            hc.TakeDamage(shot.GetDamage());
            if (proj.CompareTag("Enemy"))
                SpawnManager.Instance.decrementNumAliveEnemies();
            Destroy(proj);

            if (hc.currentHealth <= 0)
            {
                CoordinateManager.Instance.occupyCoordinate(CoordinateManager.Instance.getNearestWorldPosCoordinate(transform.position), PathManager.Instance.pathTileMap.ContainsKey(CoordinateManager.Instance.getNearestWorldPosCoordinate(transform.position)) ? OccupationType.Path : OccupationType.None);
                Destroy(gameObject);
            }
        }
    }
    public int GetCost()
    {
        return cost;
    }
}
