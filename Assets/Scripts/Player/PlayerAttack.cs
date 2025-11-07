using System.IO.Pipes;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int damage = 1; //Default is 1;
    [SerializeField] private float hitboxActiveTime;

    [Header("Other Components")]
    [SerializeField] private GameObject hitbox;
    [SerializeField] private GameObject attackExplosion;
    [SerializeField] private Animator animator;

    // Private bits
    private bool isAttacking = false;
    private float hitboxTimer;

    private PlayerHotBarManager playerHotBar;
    private PlaceManager placeManager;

    void Start()
    {
        hitbox.SetActive(false); //By default is off

        hitboxTimer = hitboxActiveTime;

        playerHotBar = GetComponent<PlayerHotBarManager>();
        placeManager = GetComponent<PlaceManager>();
    }

    // Honestly this code is pretty bad but I'm scared to do doing events
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) {
            if (!isAttacking && !playerHotBar.isInBuildMode())
            {
                startAttacking();
            }
        }
    }

    void FixedUpdate()
    {
        if (!isAttacking) { return; }

        if (hitboxTimer <= 0) { stopAttacking(); }

        hitboxTimer -= Time.fixedDeltaTime;
    }

    void startAttacking()
    {
        isAttacking = true;
        hitbox.SetActive(true);
        hitboxTimer = hitboxActiveTime;

        Instantiate(attackExplosion, placeManager.getWorldPlacingCoord(), Quaternion.identity);
        animator.SetTrigger("Attack");
    }
    void stopAttacking()
    {
        isAttacking = false;
        hitbox.SetActive(false);
        hitboxTimer = hitboxActiveTime; //This is a redudancy lol i don't need it here
    }
}
