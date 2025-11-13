using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class BaseController : MonoBehaviour
{
    public static BaseController Instance;
    public GameObject gameOverCanvas;
    public GameObject gameOverText;
    public GameObject resumeButton;
    private HealthController hc;
    private bool hasLostHalfHealth = false;

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
        hc = GetComponent<HealthController>();
    }

    void Update()
    {
        if(hc.currentHealth < hc.maxHealth * 2 / 3 && !hasLostHalfHealth)
        {
            TutorialManager.Instance.QueuePrompt("lowHealth");
            hasLostHalfHealth = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject proj = other.gameObject;
        Projectile projectileComponent = proj.GetComponent<Projectile>();
        if (projectileComponent == null || projectileComponent.target == null || !projectileComponent.target.Equals(gameObject))
            return;

        if (hc != null)
        {

            Projectile shot = (Projectile)proj.GetComponent(typeof(Projectile));
            AudioManager.Instance.PlaySFX("base hit", 0.2f, 0.6f, 1.3f);
            hc.TakeDamage(shot.GetDamage());
            if (proj.CompareTag("Enemy"))
                SpawnManager.Instance.decrementNumAliveEnemies();
            Destroy(proj);

            if (hc.currentHealth <= 0)
            {
                GameOver();
            }
        }
    }
    
    public void GameOver()
    {
        int waveReached = SpawnManager.Instance.numCurrentWave;
        int coinCollected = FindObjectOfType<PlayerController>().totalCoins;
        int highestWave = PlayerPrefs.GetInt("MaxWave", 0);
        int highestCoin = PlayerPrefs.GetInt("MaxCoin", 0);
        if (waveReached > highestWave)
        {
            PlayerPrefs.SetInt("MaxWave", waveReached);  // save new high score
        }
        if (coinCollected > highestCoin)
        {
            PlayerPrefs.SetInt("MaxCoin", highestCoin);
        }
        PlayerPrefs.Save();
        // Show UI and pause game
        gameOverText.GetComponent<TextMeshProUGUI>().text = "Game Over!\nYou survived " + waveReached + " waves!\nHigh score: "+ highestWave +"\nYou collected " + coinCollected + " coins!\nHigh score: "+highestCoin;
        gameOverCanvas.SetActive(true);
        resumeButton.SetActive(false);
        
        Time.timeScale = 0f; 
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        resumeButton.SetActive(true);

        Destroy(AudioManager.Instance.gameObject);
        Destroy(PlayerHotBarManager.Instance.gameObject);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
