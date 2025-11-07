using Unity.VisualScripting;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //other.GetComponent<PlayerController>()

            Destroy(gameObject);
        }
    }
}
