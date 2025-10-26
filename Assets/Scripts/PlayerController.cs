using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("PlayerStats")]
    [SerializeField] private float speed = 10f; //[SerializeField] makes it show up in the editor but doesn't make it public to other scripts

    private Rigidbody rb;
    private Vector3 input;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update() // Input called in the update
    {
        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    }
    void FixedUpdate() //Actual movement in fixed update so isn't frame dependant
    {
        
        if (input != Vector3.zero)
        {

            Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            Vector3 heading = Vector3.Normalize(dir * speed * Time.fixedDeltaTime);

            transform.forward = heading;
            transform.position += dir * speed * Time.fixedDeltaTime;
        }
    }

}
