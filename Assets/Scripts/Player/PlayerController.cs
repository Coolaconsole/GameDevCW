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
    private Rigidbody rb;
    private Vector3 input;
    public GameObject tower1;
    public GameObject tower2;
    public GameObject tower3;
    private GameObject currentTower;
    private bool canPlaceTower = true;
    private float placeCooldown = 1f;
    private float timeplaceCooldown = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTower = tower1;
        rb = GetComponent<Rigidbody>();
    }

    void Update() // Input called in the update
    {
        // Movement
        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        PlayerRotation();
        
        EvalTowerPlacement();
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
    }
    
    void EvalTowerPlacement()
    {
        // If it can't place a tower, start countdown
        if (!canPlaceTower)
        {
            timeplaceCooldown += Time.deltaTime;
            if (timeplaceCooldown >= placeCooldown)
            { // Reset cooldown
                Debug.Log("Tower placement ready");
                canPlaceTower = true;
                timeplaceCooldown = 0f;
            }
        }

        int lastAngle = 0;
        switch(transform.rotation.eulerAngles.y)
        { // Keeps track of last direction facing
            case 0:
                lastAngle = 0;
                break;
            case 90:
                lastAngle = 90;
                break;
            case 180:
                lastAngle = 180;
                break;
            case 270:
                lastAngle = 270;
                break;
        }
        // Selecting a different tower
        if (Input.GetKey(KeyCode.Alpha1))
        {
            currentTower = tower1;
            Debug.Log("Tower 1 selected");
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            currentTower = tower2;
            Debug.Log("Tower 2 selected");
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            currentTower = tower3;
            Debug.Log("Tower 3 selected");
        }
        if (Input.GetKey(KeyCode.Space) && canPlaceTower)
        {
            Vector3 placingDir = new Vector3(0, 0, 0);
            switch (lastAngle)
            {
                case 0:
                    placingDir = new Vector3(0, 0, 2);
                    Debug.Log("Placing tower in left");
                    break;
                case 90:
                    placingDir = new Vector3(2, 0, 0);
                    Debug.Log("Placing tower to the up");
                    break;
                case 180:
                    placingDir = new Vector3(0, 0, -2);
                    Debug.Log("Placing tower to the right");
                    break;
                case 270:
                    placingDir = new Vector3(-2, 0, 0);
                    Debug.Log("Placing tower down");
                    break;
            }
            var placeCoord = CoordinateManager.Instance.getNearestWorldPosCoordinate(transform.position + placingDir);
            Vector3 placePos = CoordinateManager.Instance.getCoordinateWorldPos(placeCoord);
            Instantiate(currentTower, placePos, Quaternion.identity);
            canPlaceTower = false;
        }
    }
}
