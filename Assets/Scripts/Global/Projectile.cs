using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject target;
    private float ttl = 10f;
    public float speed = 10f;
    public int damage = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void SetTarget(GameObject o)
    {
        target = o;
    }

    public void SetDamage(int d)
    {
        damage = d;
    }
    public int GetDamage()
    {
        return damage;
    }

    void Update()
    {
        ttl -= Time.deltaTime;
        if (ttl < 0)
        {
            if (CompareTag("Enemy"))
                SpawnManager.Instance.decrementNumAliveEnemies();
            Destroy(gameObject);
        }

        if (target == null)
        {
            if (CompareTag("Projectile"))
                Destroy(gameObject);
            return;
        }else
        {
            transform.LookAt(target.transform);
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);


            // projectile close to target but not destroed
            if (Vector3.Distance(transform.position, target.transform.position) < 0.05f)
            {
                if (CompareTag("Enemy"))
                    SpawnManager.Instance.decrementNumAliveEnemies();
                Destroy(gameObject);
                
            }
        }

        
    }
}
