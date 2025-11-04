using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class PlayerController : MonoBehaviour
{
    [Header("PlayerStats")]
    [SerializeField] private float speed = 10f; //[SerializeField] makes it show up in the editor but doesn't make it public to other scripts

    [Header("Other Components")]
    [SerializeField] private Camera camera;
    [SerializeField] private LayerMask ground;
    private Rigidbody rb;
    private Vector3 input;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update() // Input called in the update
    {
        // Movement
        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));


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

        if(Physics.Raycast(raycast, out hit, Mathf.Infinity, ground))
        {
            Vector3 lookDir = hit.point - transform.position;
            lookDir.y = 0;

            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

}
