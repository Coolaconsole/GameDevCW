using UnityEngine;

public class TowerProjectile : MonoBehaviour
{
    public GameObject target;
    private float ttl = 5f;
    public float speed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void SetTarget(GameObject o){
        target = o;
        Debug.Log(target.tag);
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
            //GetComponent<Rigidbody>().MovePosition(pos);
        } else
        {
            Debug.Log("Don' question it, nothing's wrong");
        }
        
    }
}
