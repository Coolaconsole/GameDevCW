using UnityEngine;
using System.Linq;

public class AreaTower : DefaultTower
{
    void Update()
    {
        targetController.UpdateTarget();

        timeshootCooldown += Time.deltaTime;
        if (timeshootCooldown >= shootCooldown)
        {
            foreach (var target in targetController.possibleTargets)
            {

                AudioManager.Instance.PlaySFX("tower shoot", 0.2f, 0.5f, 1.2f);
                if (target == null) continue;  // clean up destroyed objects
                else if (target.tag == "Enemy")
                {

                    Projectile proj = Instantiate(projectile, transform.position + new Vector3(0, 1, 0), Quaternion.identity).GetComponent<Projectile>();
                    proj.SetDamage(damage);
                    proj.SetTarget(target);
                    timeshootCooldown = 0f;
                }
                
            }

        }
    }
}
