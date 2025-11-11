using UnityEngine;

public class TowerAiming : MonoBehaviour
{
    [SerializeField] private GameObject head;
    
    private TargetController targetController;

    private Transform defaultPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetController = GetComponentInChildren<TargetController>();
        defaultPosition = head.transform;
    }

    // Update is called once per frame
    void Update()
    {
        //If we have a target face the target
        if (targetController.currentTarget != null)
        {
            head.transform.LookAt(targetController.currentTarget.transform);
        }
        else
        {
            head.transform.rotation = defaultPosition.rotation;
        }
    }
}
