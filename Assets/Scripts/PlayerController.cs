using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 input;
    public float speed = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (input != Vector3.zero)
        {
            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
            var rotatedInput = matrix.MultiplyPoint3x4(input);

            var relative = (transform.position + rotatedInput) - transform.position;
            var rotation = Quaternion.LookRotation(relative, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, 45);
        }

        rb.MovePosition(transform.position + (transform.forward * input.magnitude) * speed * Time.deltaTime);
        //transform.position += input;
    }
    
    void FixedUpdate()
    {
        
    }
}
