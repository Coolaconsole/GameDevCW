using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using System;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("PlayerStats")]
    [SerializeField] private float speed = 10f; //[SerializeField] makes it show up in the editor but doesn't make it public to other scripts
    public int coinCount;
    [SerializeField] private float knockbackForce = 10f;

    [Header("Other Components")]
    [SerializeField] private Camera camera;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask ground;
    [SerializeField] private Animator animator;
    private Vector3 input;
    
    public GameObject pauseScreen;


    void Update() // Input called in the update
    {
        //Pause upon pressing Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0; //Pause
                pauseScreen.SetActive(true);
            }
            else
            {
                Time.timeScale = 1; //Unpause
                pauseScreen.SetActive(false);
            }
        }
        // Movement
        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //Animation
        animator.SetFloat("moveSpeed", Mathf.Abs(input.x) + Mathf.Abs(input.z));

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

    public void UpdateCoinCount(int value) 
    {
        coinCount += value;
        GetComponent<PlayerHotBarManager>().onCoinCountChanged?.Invoke(coinCount);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            HitByEnemy(other.gameObject);
        }
    }

    private void HitByEnemy(GameObject enemy)
    {
        Vector3 direction = enemy.transform.position - transform.position;
        rb.AddExplosionForce(knockbackForce, enemy.transform.position, direction.magnitude);
    }
}
