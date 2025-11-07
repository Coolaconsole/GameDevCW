using UnityEngine;

public class Cloud : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + moveSpeed * Time.deltaTime);
    }
}
