using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class HotbarItemChangedEvent : UnityEvent<GameObject> { }
[System.Serializable]
public class PlayerBuildModeChangedEvent : UnityEvent<bool> { }
public class PlayerHotBarManager : MonoBehaviour
{
    [Header("Towers")]
    [SerializeField] private List<GameObject> towers = new List<GameObject>();

    [Header("Placing Stats")]
    [SerializeField] private float placeCooldown = 1f;
    
    [Header("UI Elements")]
    [SerializeField] private List<GameObject> HotbarDisplayUI = new List<GameObject>();
    private int currentTowerIndex = -1;

    [Header("Other Components")]
    [SerializeField] private Animator anim;

    private GameObject currentTower; //Will be null if no tower is selected (player attack mode)
    private bool buildMode = false; //Only true when the player has a building selected, if false assumes is in attacking mode
    private bool canPlaceTower = true;
    private float timeplaceCooldown = 0f;
    private PlaceManager placingManager;
    public HotbarItemChangedEvent onHotbarItemChanged;
    public PlayerBuildModeChangedEvent onBuildModeChanged;

    void Start()
    {
        currentTower = towers[0];
        placingManager = GetComponent<PlaceManager>();
    }

    // Update is called once per frame
    void Update()
    {
        SelectTower(); //Checks if player switches to a tower
        TowerCooldown(); //Handles tower placement cooldown
        //if (EvalTowerPlacement()) //Can They place a tower?
        if (buildMode)
        {
            PlaceTower();
        } else
        {
            // Attack mode logic can go here
            
        }
    }

    private void SelectTower()
    {
        bool currentBuildMode = buildMode;
        // Selecting a different tower
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentTower = towers[0];
            buildMode = true;
            ScaleUI(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentTower = towers[1];
            buildMode = true;
            ScaleUI(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentTower = towers[2];
            buildMode = true;
            ScaleUI(2);

        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentTower = towers[3];
            buildMode = true;
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
            buildMode = false;
        else { return; }
        onHotbarItemChanged.Invoke(currentTower); //If the item was changed, invoke the event
        if (buildMode != currentBuildMode) { onBuildModeChanged.Invoke(buildMode); } //If build mode has changed, then invoke the event
    }

    private void TowerCooldown()
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
        //return canPlaceTower;
    }

    private void PlaceTower()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && canPlaceTower)
        {
            //Placing direction handled by the placing manager and shown with the hover indicator
            Vector3 placePos = CoordinateManager.Instance.getCoordinateWorldPos(placingManager.getPlacingCoord());
            Instantiate(currentTower, placePos, Quaternion.identity);
            canPlaceTower = false;

            anim.SetTrigger("Attack"); //Looks like they are placing it down!
        }
    }

    // Functions for the outside world ---------------------------------

    public bool isInBuildMode() { return buildMode; }

    /// <summary>
    /// Will return null if the player is not in build mode
    /// </summary>
    /// <returns></returns>
    public GameObject getCurrentTower() { return currentTower; }

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
