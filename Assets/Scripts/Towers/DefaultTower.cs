using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefaultTower : MonoBehaviour
{
    [SerializeField] private int health = 100;
    public GameObject projectile;
    public int damage = 10;
    public float shootCooldown = 1f;
    protected TowerProjectile script;
    protected List<GameObject> targets;
    protected GameObject currentTarget;
    protected float timeshootCooldown = 0f;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = 100;
        targets = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        timeshootCooldown += Time.deltaTime;
        if (timeshootCooldown >= shootCooldown)
        {
            if (currentTarget != null)
            {
                Instantiate(projectile, transform.position + new Vector3(0,1,0), Quaternion.identity);
                script = (TowerProjectile)projectile.GetComponent(typeof(TowerProjectile));
                script.SetTarget(currentTarget.gameObject);
                script.SetDamage(damage);
                timeshootCooldown = 0f;
            }
            
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            targets.Add(other.gameObject);
            UpdateTarget();
            Debug.Log("COLLISION");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            targets.Remove(other.gameObject);
            if (currentTarget.Equals(other.gameObject))
            {
                currentTarget = null;
            }
            UpdateTarget();
        }
    }
    // Function inspired by https://www.youtube.com/watch?v=XsGHjZ1R3fI
    protected void UpdateTarget()
    {
        if (currentTarget != null)
        {
            return;
        }
        GameObject closestEnemy = null;
        float closestDist = float.MaxValue;

        foreach (GameObject item in targets)
        {
            if(item != null)
            {
                float dist = Vector3.Distance(transform.position, item.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestEnemy = item;
                }
            }
        }
        
        if (closestEnemy != null)
        {
            currentTarget = closestEnemy;
        } else
        {
            currentTarget = null;
        }
    }

}
