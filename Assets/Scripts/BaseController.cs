using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class BaseController : MonoBehaviour
{
    public static BaseController Instance;
    public GameObject gameOverCanvas;
    public GameObject gameOverText;
    public GameObject resumeButton;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameOverCanvas.SetActive(false);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CoordinateManager.Instance.occupyCoordinate(CoordinateManager.Instance.getNearestWorldPosCoordinate(transform.position), OccupationType.Base);
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject proj = other.gameObject;
        Projectile projectileComponent = proj.GetComponent<Projectile>();
        if (projectileComponent == null || projectileComponent.target == null || !projectileComponent.target.Equals(gameObject))
            return;

        HealthController hc = GetComponent<HealthController>();
        if (hc != null)
        {

            Projectile shot = (Projectile)proj.GetComponent(typeof(Projectile));
            hc.TakeDamage(shot.GetDamage());
            Destroy(proj);

            if (hc.currentHealth <= 0)
            {
                GameOver();
            }
        }
    }
    
    public void GameOver()
    {
        // Show UI and pause game
        gameOverText.GetComponent<TextMeshProUGUI>().text = "Game Over!\nYou survived " + SpawnManager.Instance.numCurrentWave.ToString() + " waves!\nYou collected " + FindObjectOfType<PlayerController>().coinCount.ToString() + " coins!";
        gameOverCanvas.SetActive(true);
        resumeButton.SetActive(false);
        Time.timeScale = 0f; 
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        resumeButton.SetActive(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        resumeButton.SetActive(true);
        Application.Quit();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; 
        gameOverCanvas.SetActive(false);
    }
}
