using Unity.VisualScripting;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    public int value = 1;

    [SerializeField] private GameObject coinDeathParticles;
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().UpdateCoinCount(value);
            
            TutorialManager.Instance.onMoneyPickup.Invoke(); //Tells the tutorial manager we MANAGED to do it (lol sorry I've been coding for too long)
            
            Death();
        }
    }
    void Update()
    {
        if (value <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void Death()
    {
        Instantiate(coinDeathParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
