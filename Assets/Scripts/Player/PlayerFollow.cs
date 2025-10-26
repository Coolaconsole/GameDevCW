using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public GameObject target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 offset = new Vector3(-4, 3, -4);
        transform.position = target.transform.position + offset;
    }
}
