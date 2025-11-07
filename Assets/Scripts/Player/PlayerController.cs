using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using System;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("PlayerStats")]
    [SerializeField] private float speed = 10f; //[SerializeField] makes it show up in the editor but doesn't make it public to other scripts

    [Header("Other Components")]
    [SerializeField] private Camera camera;
    [SerializeField] private LayerMask ground;
    private Rigidbody rb;
    private Vector3 input;
    public List<GameObject> towers = new List<GameObject>();
    public List<GameObject> HotbarDisplayUI = new List<GameObject>();
    private int currentTowerIndex = -1;
    private GameObject currentTower;
    private bool canPlaceTower = true;
    private float placeCooldown = 1f;
    private float timeplaceCooldown = 0f;
    private PlacingManager placingManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTower = towers[0];
        rb = GetComponent<Rigidbody>();

        placingManager = GetComponent<PlacingManager>();
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
            if (transform.position.x < -24f)
                transform.position = new Vector3(-24f, transform.position.y, transform.position.z);
            else if (transform.position.x > 24f)
                transform.position = new Vector3(24f, transform.position.y, transform.position.z);
            if (transform.position.z < -24f)
                transform.position = new Vector3(transform.position.x, transform.position.y, -24f);
            else if (transform.position.z > 24f)
                transform.position = new Vector3(transform.position.x, transform.position.y, 24f);
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
        // Selecting a different tower
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentTower = towers[0];
            ScaleUI(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentTower = towers[1];
            ScaleUI(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentTower = towers[2];
            ScaleUI(2);

        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentTower = towers[3];
            ScaleUI(3);
        }
        // if (Input.GetKeyDown(KeyCode.Alpha5))
        // {
        //     currentTower = towers[2];
        //     ScaleUI(4);
        // }
        // if (Input.GetKeyDown(KeyCode.Alpha6))
        // {
        //     currentTower = towers[3];
        //     ScaleUI(5);
        // }

         if (currentTowerIndex == -1)
            //Attacking if no tower is selected

        if (Input.GetKeyDown(KeyCode.Space) && canPlaceTower)
        {
            //Placing direction handled by the placing manager and shown with the hover indicator
            Vector3 placePos = CoordinateManager.Instance.getCoordinateWorldPos(placingManager.getPlacingCoord());
            Instantiate(currentTower, placePos, Quaternion.identity);
            canPlaceTower = false;
        }
    }
    
    void ScaleUI(int index)
    {
        if (index == currentTowerIndex)
        {
            HotbarDisplayUI[index].transform.localScale = new Vector3(1f, 1f, 1f);
            currentTowerIndex = -1;
        }
        else if (currentTowerIndex == -1)
        {
            HotbarDisplayUI[index].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            currentTowerIndex = index;
        }
        else
        {
            HotbarDisplayUI[currentTowerIndex].transform.localScale = new Vector3(1f, 1f, 1f);
            HotbarDisplayUI[index].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            currentTowerIndex = index;
        }
    }
}
