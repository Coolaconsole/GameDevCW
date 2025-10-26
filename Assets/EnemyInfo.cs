using UnityEngine;

public class EnemyInfo : MonoBehaviour
{

    [Header("Stats")]
    [SerializeField] private int health = 1;

    [Header("Other Components")]
    [SerializeField] private Collider playerHitBox;
    void OnTriggerEnter(Collider other)
    {
        if (other == playerHitBox)
        {
            // Player has hit the enemy
            enemyHit();
        }
    }

    private void enemyHit()
    {
        health -= 1;
        checkEnemyDeath();
    }

    private void checkEnemyDeath()
    {
        if (health <= 0)
        {
            Destroy(gameObject); //Enemy is removed from this world
        }
    }
}
