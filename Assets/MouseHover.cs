using System;
using UnityEngine;

public class MouseHover : MonoBehaviour
{
    [Header("Other Components")]
    [SerializeField] private Camera camera;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject hoverIndicatorPrefab;

    private GameObject hoverIndicator;

    private Vector3 lastPosition;
    // Update is called once per frame

    private void Start()
    {
        hoverIndicator = Instantiate(hoverIndicatorPrefab);
    }
    void Update()
    {
        Vector3 mousePos = GetSelectedMapPos();

        //Using floor to do the grid visualisation
        hoverIndicator.transform.position = CoordinateManager.Instance.getCoordinateWorldPos(CoordinateManager.Instance.getNearestWorldPosCoordinate(new Vector3(mousePos.x, 0, mousePos.z))) + new Vector3(0, 0.015f, 0);//new Vector3(Mathf.Floor(mousePos.x*0.5f)*2, 0.005f, Mathf.Floor(mousePos.z*0.5f)*2);
    }
    
    public Vector3 GetSelectedMapPos()
    {
        Vector3 mousePos = Input.mousePosition;

        Ray ray = camera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, groundLayer))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }
}
