using Unity.VisualScripting;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    public int value = 1;
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().UpdateCoinCount(value);

            Destroy(gameObject);
        }
    }
    void Update()
    {
        if (value <= 0)
        {
            Destroy(gameObject);
        }
    }
}
