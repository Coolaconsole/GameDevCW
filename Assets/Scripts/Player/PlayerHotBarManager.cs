using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class HotbarItemChangedEvent : UnityEvent<GameObject> { }
[System.Serializable]
public class PlayerBuildModeChangedEvent : UnityEvent<bool> { }
public class PlayerCoinCountChangedEvent : UnityEvent<int> { }
public class PlayerHotBarManager : MonoBehaviour
{
    [Header("Towers")]
    [SerializeField] private List<GameObject> towers = new List<GameObject>();

    [Header("Placing Stats")]
    [SerializeField] private float placeCooldown = 1f;
    
    [Header("UI Elements")]
    [SerializeField] private List<GameObject> HotbarDisplayUI = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI coinCountText;
    [SerializeField] private TextMeshProUGUI tooltipText;
    private int currentTowerIndex = -1;

    [Header("Other Components")]
    [SerializeField] private Animator anim;

    private GameObject currentTower; //Will be null if no tower is selected (player attack mode)
    private bool buildMode = false; //Only true when the player has a building selected, if false assumes is in attacking mode
    private bool canPlaceTower = true;
    private float timeplaceCooldown = 0f;
    private PlaceManager placingManager;
    public HotbarItemChangedEvent onHotbarItemChanged = new HotbarItemChangedEvent();
    public PlayerBuildModeChangedEvent onBuildModeChanged = new PlayerBuildModeChangedEvent();
    public PlayerCoinCountChangedEvent onCoinCountChanged = new PlayerCoinCountChangedEvent();
    void Start()
    {
        currentTower = towers[0];
        placingManager = GetComponent<PlaceManager>();

        onCoinCountChanged.AddListener(UpdateCoinCount);
        UpdateCoinCount(19); //Initial update
    }

    // Update is called once per frame
    void Update()
    {
        SelectTower(); //Checks if player switches to a tower
        TowerCooldown(); //Handles tower placement cooldown
        //if (EvalTowerPlacement()) //Can They place a tower?
        if (buildMode && CanCostTower(currentTower))
        {
            PlaceTower();
        } else
        {
            tooltipText.text = "Not enough coins to place this tower!";
        }
    }

    private void SelectTower()
    {
        bool currentBuildMode = buildMode;
        int previousTowerIndex = currentTowerIndex;
        // Selecting a different tower
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentTower = towers[0];
            buildMode = true;
            ScaleUI(0);
            tooltipText.text = "Basic Tower - Cost: " + currentTower.GetComponent<DefaultTower>().cost.ToString() + "\nRange: Average - Damage: Low";
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentTower = towers[1];
            buildMode = true;
            ScaleUI(1);
            tooltipText.text = "Fast-shooting Tower - Cost: " + currentTower.GetComponent<DefaultTower>().cost.ToString() + "\nRange: Low - Damage: Average";
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentTower = towers[2];
            buildMode = true;
            ScaleUI(2);
            tooltipText.text = "Long-range Tower - Cost: " + currentTower.GetComponent<DefaultTower>().cost.ToString() + "\nRange: High - Damage: High";
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentTower = towers[3];
            buildMode = true;
            ScaleUI(3);
            tooltipText.text = "Area Damage Tower - Cost: " + currentTower.GetComponent<DefaultTower>().cost.ToString() + "\nRange: Low - Damage: Low (Area Effect)";
        }

        if (currentTowerIndex == -1)
        { 
            buildMode = false;
            currentTower = null;
            tooltipText.text = "Attack Mode:\nLeft Click or SPACE to Attack";
        }
        //else { return; }
        if (previousTowerIndex != currentTowerIndex){onHotbarItemChanged.Invoke(currentTower);} //If the item was changed, invoke the event
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
            OccupationType placePosType = CoordinateManager.Instance.getCoordinateOccupation(placingManager.getPlacingCoord());
            if (placePosType == OccupationType.Base || placePosType == OccupationType.Tower)
                return;
            Instantiate(currentTower, placePos, Quaternion.identity);
            canPlaceTower = false;
            CoordinateManager.Instance.occupyCoordinate(placingManager.getPlacingCoord(), OccupationType.Tower);
            if (anim != null)
                anim.SetTrigger("Attack"); //Looks like they are placing it down!
            SpendCoin(currentTower.GetComponent<DefaultTower>().cost);
        }
    }
    
    private bool CanCostTower(GameObject tower)
    {
        int cost = tower.GetComponent<DefaultTower>().cost;
        int currentCoins = GetComponent<PlayerController>().coinCount;
        return currentCoins >= cost;
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

    void SpendCoin(int value)
    {
        GetComponent<PlayerController>().UpdateCoinCount(-value);
    }

    void UpdateCoinCount(int count)
    {
        int coinCount = GetComponent<PlayerController>().coinCount;
        coinCountText.text = coinCount.ToString();

        for (int i = 0; i < towers.Count; i++)
        {
            GameObject tower = towers[i];
            int cost = tower.GetComponent<DefaultTower>().cost;
            if (cost > coinCount)
            {
                int childCount = HotbarDisplayUI[i].transform.childCount;
                for (int j = 0; j < childCount; j++)
                {
                    GameObject uiElement = HotbarDisplayUI[i].transform.GetChild(j).gameObject;
                    uiElement.GetComponent<CanvasRenderer>().SetAlpha(0.2f);
                }
            } else 
            {
                int childCount = HotbarDisplayUI[i].transform.childCount;
                for (int j = 0; j < childCount; j++)
                {
                    GameObject uiElement = HotbarDisplayUI[i].transform.GetChild(j).gameObject;
                    uiElement.GetComponent<CanvasRenderer>().SetAlpha(1f);
                }
            }
        }
    }
}
