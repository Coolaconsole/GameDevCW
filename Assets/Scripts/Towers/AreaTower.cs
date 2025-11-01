using UnityEngine;
using System.Linq;

public class AreaTower : DefaultTower
{

    //Attacks each target in range
    void Update()
    {
        timeshootCooldown += Time.deltaTime;
        if (timeshootCooldown >= shootCooldown && targets.Any())
        {
            foreach (GameObject target in targets) if (target != null)
            {
                Instantiate(projectile, transform.position + new Vector3(0,1,0), Quaternion.identity);
                script = (TowerProjectile)projectile.GetComponent(typeof(TowerProjectile));
                script.SetTarget(target.gameObject);
                script.SetDamage(damage);
                timeshootCooldown = 0f;
            }
            
        }
    }
}
