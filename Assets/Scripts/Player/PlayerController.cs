using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("PlayerStats")]
    [SerializeField] private float speed = 10f; //[SerializeField] makes it show up in the editor but doesn't make it public to other scripts

    [Header("Other Components")]
    [SerializeField] private Camera camera;
    [SerializeField] private LayerMask ground;
    [SerializeField] private Animator animator;
    private Vector3 input;

    void Update() // Input called in the update
    {
        // Movement
        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //Animation
        animator.SetFloat("moveSpeed", Mathf.Abs(input.x) + Mathf.Abs(input.z));

        PlayerRotation();
    }
    void FixedUpdate() //Actual movement in fixed update so isn't frame dependant
    {
        //Handles the movement 

        if (input != Vector3.zero)
        {
            //The Vector3's are added so that the player moves on a 45 degree angle
            Vector3 forward = new Vector3(1f, 0f, 1f).normalized * Input.GetAxis("Vertical");
            Vector3 right = new Vector3(1f, 0f, -1f).normalized * Input.GetAxis("Horizontal");

            transform.position += (forward + right).normalized * speed * Time.fixedDeltaTime;
        }
    }
    
    private void PlayerRotation()
    {
        Ray raycast = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(raycast, out hit, Mathf.Infinity, ground))
        {
            Vector3 lookDir = hit.point - transform.position;
            lookDir.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        /*
        if (Physics.Raycast(raycast, out hit, Mathf.Infinity, ground))
        {
            Vector3 lookDir = hit.point - transform.position;
            lookDir.y = 0;

            transform.rotation = Quaternion.LookRotation(lookDir);
            // Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            // Vector3 heading = Vector3.Normalize(dir * speed * Time.fixedDeltaTime);

            // transform.forward = heading;
            // transform.position += dir * speed * Time.fixedDeltaTime;

            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
            var rotatedInput = matrix.MultiplyPoint3x4(input);

            var relative = (transform.position + rotatedInput) - transform.position;
            var rotation = Quaternion.LookRotation(relative, Vector3.up);

            //transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, 45);
            transform.rotation = rotation;
        }

        transform.rotation = Quaternion.Euler(0, Mathf.Round(transform.rotation.eulerAngles.y / 45) * 45, 0);
        //rb.MovePosition(transform.position + (transform.forward * input.magnitude) * speed * Time.deltaTime);
        transform.position += (transform.forward * input.magnitude) * speed * Time.deltaTime;
        //transform.position += input;
        */
    }
}
