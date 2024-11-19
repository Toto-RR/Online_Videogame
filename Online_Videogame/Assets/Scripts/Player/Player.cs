using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    // Componentes esenciales
    public PlayerHealth health;
    public PlayerShoot shoot;
    public FPSController movement;

    // Información de player
    public string playerId { get; private set; }
    public string playerName { get; private set; }
    public GameConfigSO gameConfig;

    // Valores de movimiento
    public float moveSpeed = 5f;

    // Shooting 
    public float damage = 10f;
    public float shootRange = 50f;
    public int currentAmmo = 10;

    // Variables de salud
    public float maxHealth = 100;

    // Energy (to dash)
    public int currentEnergy = 4; // Able to do 4 dashes

    private PlayerHealthUI healthBar;

    private void Awake()
    {
        Instance = this;

        playerId = string.IsNullOrEmpty(gameConfig.PlayerID) ? "0A" : gameConfig.PlayerID;
        playerName = string.IsNullOrEmpty(gameConfig.PlayerName) ? "Player1" : gameConfig.PlayerName;
        Debug.Log("ID: " + playerId);
        Debug.Log("Name: " + playerName);

        // Inicializar los componentes
        health = GetComponent<PlayerHealth>();
        shoot = GetComponent<PlayerShoot>();
        movement = GetComponent<FPSController>();

        // Si el PlayerHealth no está asignado, crearlo
        if (health == null)
            health = gameObject.AddComponent<PlayerHealth>();

        // Asegurar que la salud inicial sea la máxima
        health.SetHealth(maxHealth);

        healthBar = FindAnyObjectByType<PlayerHealthUI>();
    }

    private void Update()
    {
        // Actualizar los componentes en cada frame
        movement.HandleMovement();
        shoot.HandleShooting(damage, shootRange);

        //DEBUG INPUTS
        if(Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(damage);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            Heal(10);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Current Health: " + health.GetCurrentHealth().ToString());
        }
        
    }

    // Método para aplicar daño
    public void TakeDamage(float damage)
    {
        health.TakeDamage(damage);
        healthBar.UpdateHealthBar(health.GetCurrentHealth());
    }

    // Método para curar
    public void Heal(float amount)
    {
        health.Heal(amount);
        healthBar.UpdateHealthBar(health.GetCurrentHealth());
    }

    public int GetEnergy()
    {
        return currentEnergy;
    }

    public int GetAmmoCount()
    {
        return currentAmmo;
    }
    // Métodos adicionales para movimiento, disparo, etc. se delegan a componentes como antes.
}
