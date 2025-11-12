using System.IO.Pipes;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Stats")]
    public int damage = 1; //Default is 1;
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
    private bool hasAttackedBefore;

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
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) && Time.timeScale == 1) {
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
        AudioManager.Instance.PlaySFX("player attack", 0.6f, 0.7f, 1.3f);

        Instantiate(attackExplosion, placeManager.getWorldPlacingCoord(), Quaternion.identity);
        animator.SetTrigger("Attack");

        if(!hasAttackedBefore) {
            TutorialManager.Instance.onPlayerAttack.Invoke();
            hasAttackedBefore = true; }
    }
    void stopAttacking()
    {
        isAttacking = false;
        hitbox.SetActive(false);
        hitboxTimer = hitboxActiveTime; //This is a redudancy lol i don't need it here
    }
}
