using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class HealthController : MonoBehaviour
{
    public int maxHealth = 100;
    [NonSerialized] public int currentHealth;

    public Image healthBarFill;
    public Gradient healthGradient;


    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
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
