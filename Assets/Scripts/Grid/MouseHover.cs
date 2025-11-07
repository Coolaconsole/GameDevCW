using NUnit.Framework.Constraints;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.AI;

public class PlaceManager : MonoBehaviour
{
    [Header("Other Components")]
    [SerializeField] private Camera camera;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject attackIndicatorPrefab;
    [SerializeField] private GameObject buildingIndicatorPrefab;
    [SerializeField] private Transform playerLookEmpty; // Used for visualising where the player is looking
    [SerializeField] private Material transparent;

    private GameObject hoverIndicator;
    private GameObject currentTower;

    private Vector2Int gridCoord;
    private Vector3 worldGridPos;
    private PlayerHotBarManager playerHotBarManager;

    // Update is called once per frame

    private void Start()
    {
        hoverIndicator = Instantiate(attackIndicatorPrefab);
        playerHotBarManager = GetComponent<PlayerHotBarManager>();

        playerHotBarManager.onHotbarItemChanged.AddListener(OnHotBarChanged); //Listens for when the hotbar item is changed
        playerHotBarManager.onBuildModeChanged.AddListener(OnBuildModeEntered);
    }
    void Update()
    {
        //Get the closest coordinate to looking empty
        gridCoord = CoordinateManager.Instance.getNearestWorldPosCoordinate(playerLookEmpty.position);

        //Get the real world pos of this coord
        worldGridPos = CoordinateManager.Instance.getCoordinateWorldPos(gridCoord);

        hoverIndicator.transform.position = worldGridPos + new Vector3(0f, 0.015f, 0f); //Add a little bit of height to avoid clipping

        if (currentTower != null) //If there is tower that should be shown
        {
            //Moves it to the correct position
            currentTower.transform.position = worldGridPos;
        }
    }

    //Called when the hotbar changes (through events)
    private void OnHotBarChanged(GameObject newItem)
    {
        if (currentTower != null)
        {
            Destroy(currentTower); //If there is another indicator still around get rid of that
        }

        if (newItem != null)
        {
            CreateBuildingIndicator(newItem);
        }
    }

    private void OnBuildModeEntered(bool buildMode)
    {
        //Get rid of the old one
        if (hoverIndicator != null) { Destroy(hoverIndicator); }
        //Get the new one up!
        if (!buildMode)             {hoverIndicator = Instantiate(attackIndicatorPrefab);}
        else                        {hoverIndicator = Instantiate(buildingIndicatorPrefab);}
    }


    //Show transparent building that is about to be placed
    private void CreateBuildingIndicator(GameObject building)
    {
        //Need to strip off all the gameplay features and just get the mesh renderers
        currentTower = Instantiate(building, worldGridPos, Quaternion.identity);

        //Remove all components the parent except visual ones
        foreach (Component component in currentTower.GetComponents<Component>())
        {
            if (component is Transform) { continue; }
            else { TurnOffComponent(component); }
        }
        //Do the same as above but for all the children
        foreach (Transform child in currentTower.GetComponentsInChildren<Transform>())
        {
            foreach (Component component in child.GetComponents<Component>())
            {
                if (component is Renderer renderer)
                {
                    renderer.material = transparent;
                    continue;
                }
                else if (component is Transform || component is MeshRenderer || component is MeshFilter) { continue; }

                else { TurnOffComponent(component); }
            }
        }
    }


    private void TurnOffComponent(Component component)
    {
        if (component is Behaviour behaviour)
            behaviour.enabled = false;
        else if (component is Collider collider)
            collider.enabled = false;
    }

    //Function for the outside world

    /// <summary>
    /// Gets the coordinate with respect to the grid
    /// </summary>
    /// <returns></returns>
    public Vector2Int getPlacingCoord() { return gridCoord; }
    
    /// <summary>
    /// Returns the real world position of the current placing coordinate
    /// </summary>
    /// <returns></returns>
    public Vector3 getWorldPlacingCoord() { return worldGridPos; }
    
}
