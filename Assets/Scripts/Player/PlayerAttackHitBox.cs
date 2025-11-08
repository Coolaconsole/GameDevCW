using UnityEngine;

public class PlayerAttackHitBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<HealthController>().TakeDamage(GetComponentInParent<PlayerAttack>().damage);
        }
    }
}
