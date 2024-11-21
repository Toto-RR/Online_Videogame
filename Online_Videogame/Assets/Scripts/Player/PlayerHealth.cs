using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private float currentHealth;
    private float maxHealth;

    public bool isDead = false;

    public event System.Action<float> OnHealthChanged;
    public event System.Action OnPlayerDeath;

    public TakeDamageUI takeDamageUI;
    public GameObject deathCanvas;
    public PlayerHealthUI healthBar;
    public ConsoleUI consoleUI;

    private void Start()
    {
        consoleUI = FindAnyObjectByType<ConsoleUI>();

    }
    // Setear la salud
    public void SetHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;

        // Make sure death canvas is not activated
        if (deathCanvas != null)
            deathCanvas.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            Die();
        }

        healthBar.UpdateHealthBar(GetCurrentHealth());
        takeDamageUI.GetTakedamage();
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
        if (!isDead) return;

        PlayerSync.Instance.HandleDie();
        OnPlayerDeath?.Invoke();
        StartCoroutine(RespawnTimer(3f));
    }

    private IEnumerator RespawnTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Respawn();
    }

    private void Respawn()
    {
        if (deathCanvas != null)
        {
            deathCanvas.SetActive(false);
        }

        SetHealth(maxHealth);
        healthBar.UpdateHealthBar(GetCurrentHealth());
        Player.Instance.SetAtRespawn();

        PlayerSync.Instance.HandleRespawn(GetRespawnPos(), GetRespawnRot(), maxHealth);
        Debug.Log("Preparando mensaje respawn...");
        Debug.Log(GetRespawnPos() + " " + GetRespawnRot());

        isDead = false;
    }

    private Vector3 GetRespawnPos()
    {
        if (Player.Instance.gameConfig.RespawnPos != null)
            return Player.Instance.gameConfig.RespawnPos;

        else return Vector3.zero;
    }

    private Quaternion GetRespawnRot()
    {
        if (Player.Instance.gameConfig.RespawnRot == null)
            return Player.Instance.gameConfig.RespawnRot;

        else return Quaternion.identity;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}