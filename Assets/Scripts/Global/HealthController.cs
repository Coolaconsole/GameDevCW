using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class OnTakeDamage : UnityEvent { }
public class HealthController : MonoBehaviour
{
    public int maxHealth = 25;
    [NonSerialized] public int currentHealth;

    public Image healthBarFill;
    public Gradient healthGradient;

    private bool isHealing = false;
    private int targetHealth = 0;
    private bool canHeal = true;
    
    [HideInInspector] public UnityEvent OnTakeDamage;


    private void Start()
    {
        
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        UpdateHealthUI();
    }

    void FixedUpdate()
    {
        if (gameObject.CompareTag("Tower"))
        {
            // Listener for if wave ends to heal towers
            if (SpawnManager.Instance.waveInProgress == false && canHeal)
            {
                isHealing = true;
                targetHealth = (maxHealth - currentHealth) / 2 + currentHealth; // Heal 50% of missing health
            }
        }
        if (isHealing)
        {
            canHeal = false;
            if (currentHealth < targetHealth)
            {
                currentHealth += 1;
            }
            else
            {
                isHealing = false;
            }
        }
        if (SpawnManager.Instance.waveInProgress)
        {
            canHeal = true;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
        
        OnTakeDamage.Invoke();
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            float fillValue = (float)currentHealth / maxHealth;
            healthBarFill.rectTransform.localScale = new Vector3 (Mathf.Max(fillValue, 0), 1, 1);
            healthBarFill.color = healthGradient.Evaluate(fillValue);
        }
    }
}
