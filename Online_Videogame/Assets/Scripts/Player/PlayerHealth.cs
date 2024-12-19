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

    // Audio variables
    public AudioClip takeDamageSound; // Sound for taking damage
    public AudioClip dieSound; // Sound for dying
    public AudioClip respawnSound; // Sound for respawn
    private AudioSource audioSource;

    private void Start()
    {
        consoleUI = FindAnyObjectByType<ConsoleUI>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
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
        if (isDead) return;

        if (currentHealth > 0)
        {
            // Play sound for taking damage
            PlaySound(takeDamageSound);

            currentHealth -= damage;
            healthBar.UpdateHealthBar(GetCurrentHealth());
            takeDamageUI.GetTakedamage();
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            healthBar.UpdateHealthBar(GetCurrentHealth());
            takeDamageUI.GetTakedamage();

            // Play sound for dying
            PlaySound(dieSound);

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
        if (isDead) return; // Verificar si ya está muerto para evitar duplicados

        isDead = true;

        PlayerSync.Instance.HandleDie(); // Enviar el mensaje de muerte
        Debug.Log("HANDLE DIE HECHO");

        OnPlayerDeath?.Invoke();

        StartCoroutine(RespawnTimer(3f));
    }

    //consoleUI.LogToConsole("INVOKE HECHO");

    //if (deathCanvas != null)
    //{
    //    consoleUI.LogToConsole("ACTIVANDO PANTALLA DE MUERTE");
    //    deathCanvas.SetActive(true);
    //}
    //else consoleUI.LogToConsole("PANTALLA DE MUERTE NULL");

    //consoleUI.LogToConsole("EMPEZANDO COROUTINA");


    private IEnumerator RespawnTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // Play sound for respawn
        PlaySound(respawnSound);

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

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

}