using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefaultTower : MonoBehaviour
{
    protected TargetController targetController;

    public GameObject projectile;
    public int damage = 10;
    public float shootCooldown = 1f;
    protected float timeshootCooldown = 0f;

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
                Projectile proj = Instantiate(projectile, transform.position + new Vector3(0, 1, 0), Quaternion.identity).GetComponent<Projectile>();
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
        if (proj.CompareTag("Projectile") && hc != null)
        {

            Projectile shot = (Projectile)proj.GetComponent(typeof(Projectile));
            hc.TakeDamage(shot.GetDamage());
            Destroy(proj);

            if (hc.currentHealth <= 0)
            { 
                Destroy(gameObject);
            }
        }
    }
}
