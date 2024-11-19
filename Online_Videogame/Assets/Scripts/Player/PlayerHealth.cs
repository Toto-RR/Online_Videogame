using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private float currentHealth;
    private float maxHealth;

    public event System.Action<float> OnHealthChanged;
    public event System.Action OnPlayerDeath;

    // Setear la salud
    public void SetHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        OnHealthChanged?.Invoke(currentHealth);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        Debug.Log("HAS MUERTO");
        PlayerSync.Instance.HandleDie();
        OnPlayerDeath?.Invoke();
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}