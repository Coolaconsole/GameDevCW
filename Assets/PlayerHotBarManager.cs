using UnityEngine;

public class PlayerHotBarManager : MonoBehaviour
{
    [Header("Towers")]
    [SerializeField] private GameObject tower1;
    [SerializeField] private GameObject tower2;
    [SerializeField] private GameObject tower3;

    [Header("Placing Stats")]
    [SerializeField] private float placeCooldown = 1f;

    private GameObject currentTower; //Will be null if no tower is selected (player attack mode)
    private bool buildMode = false; //Only true when the player has a building selected, if false assumes is in attacking mode
    private bool canPlaceTower = true;
    private float timeplaceCooldown = 0f;
    private PlaceManager placingManager;

    void Start()
    {
        placingManager = GetComponent<PlaceManager>();
    }

    // Update is called once per frame
    void Update()
    {
        SelectTower(); //Checks if player switches to a tower

        if (EvalTowerPlacement()) //Can They place a tower?
        {
            PlaceTower();
        }
    }

    private void SelectTower()
    {
        // Selecting a different tower
        if (Input.GetKey(KeyCode.Alpha1))
        {
            currentTower = null;
            buildMode = false;
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            buildMode = true;
            currentTower = tower1;
            Debug.Log("Tower 1 selected");

        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            buildMode = true;
            currentTower = tower2;
            Debug.Log("Tower 2 selected");
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            buildMode = true;
            currentTower = tower3;
            Debug.Log("Tower 3 selected");
        }
    }

    private bool EvalTowerPlacement()
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
        return canPlaceTower;
    }

    private void PlaceTower()
    {
        if (Input.GetMouseButtonDown(0) && buildMode)
        {
            //Placing direction handled by the placing manager and shown with the hover indicator
            Vector3 placePos = CoordinateManager.Instance.getCoordinateWorldPos(placingManager.getPlacingCoord());
            Instantiate(currentTower, placePos, Quaternion.identity);
            canPlaceTower = false;
        }
    }

    // Functions for the outside world ---------------------------------

    public bool isInBuildMode() { return buildMode; }
    
    /// <summary>
    /// Will return null if the player is not in build mode
    /// </summary>
    /// <returns></returns>
    public GameObject getCurrentTower() { return currentTower; }
}
