using UnityEngine;

public class BaseController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
                Debug.Log("Base destroyed!");
                // Add additional logic for base destruction here
                
            }
        }
    }
}
