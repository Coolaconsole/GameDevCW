using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlacingManager : MonoBehaviour
{
    [Header("Other Components")]
    [SerializeField] private Camera camera;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject hoverIndicatorPrefab;
    [SerializeField] private Transform playerLookEmpty; // Used for visualising where the player is looking

    private GameObject hoverIndicator;

    private Vector2Int gridCoord;

    // Update is called once per frame

    private void Start()
    {
        hoverIndicator = Instantiate(hoverIndicatorPrefab);
    }
    void Update()
    {
        //Get the closest coordinate to looking empty
        gridCoord = CoordinateManager.Instance.getNearestWorldPosCoordinate(playerLookEmpty.position);

        //Get the real world pos of this coord
        Vector3 worldGridPos = CoordinateManager.Instance.getCoordinateWorldPos(gridCoord);

        //Place the hover indicator at the position
        hoverIndicator.transform.position = worldGridPos + new Vector3(0f, 0.015f, 0f); //Add a little bit of height to avoid clipping
    }
    
    public Vector2Int getPlacingCoord(){ return gridCoord; }
}
