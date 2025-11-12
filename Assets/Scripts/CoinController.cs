using System.Data.SqlTypes;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    private int value = 1;

    [SerializeField] private GameObject coinDeathParticles;
    private Vector3 baseScale;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().UpdateCoinCount(value);

            AudioManager.Instance.PlaySFX("coin", 0.6f);
            
            TutorialManager.Instance.onMoneyPickup.Invoke(); //Tells the tutorial manager we MANAGED to do it (lol sorry I've been coding for too long)
            
            Death();
        }
    }

    private void Awake()
    {
        baseScale = transform.localScale;
        setValue(value);
    }

    public void setValue(int newValue)
    {
        value = newValue;
        float scale = 1;
        switch (value)
        {
            case 5:
                scale = 1.3f;
                break;
            case 7:
                scale = 1.5f;
                break;
            case 10:
                scale = 2f;
                break;
            case 20:
                scale = 3f;
                break;
        }

        transform.localScale = baseScale * scale;
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
