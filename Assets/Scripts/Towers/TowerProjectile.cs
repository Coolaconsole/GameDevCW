using UnityEngine;

public class TowerProjectile : MonoBehaviour
{
    public GameObject target;
    private float ttl = 5f;
    public float speed = 50f;
    public int damage = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void SetTarget(GameObject o)
    {
        target = o;
        Debug.Log(target.tag);
    }

    public void SetDamage(int d)
    {
        damage = d;
    }
    public int GetDamage()
    {
        return damage;
    }

    // Update is called once per frame
    void Update()
    {
        ttl -= Time.deltaTime;
        if(ttl < 0){
            Destroy(gameObject);
        }
    }

    void FixedUpdate(){
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        } else
        {
            Destroy(gameObject);
        }
        
    }
}
