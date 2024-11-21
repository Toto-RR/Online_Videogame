using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public PlayerHealth health { get; private set; }
    public PlayerShoot Shoot { get; private set; }
    public FPSController Movement { get; private set; }
    public string playerId { get; private set; }
    public string playerName { get; private set; }

    public GameConfigSO gameConfig;

    // Movement
    public float moveSpeed = 5f;

    // Shooting 
    public float damage = 10f;

    // Health
    public float maxHealth = 100;

    // Energy (to dash)
    public int currentEnergy = 4; // Able to do 4 dashes

    private bool isRespawning = false;

    private void Awake()
    {
        Instance = this;

        playerId = string.IsNullOrEmpty(gameConfig.PlayerID) ? "0A" : gameConfig.PlayerID;
        playerName = string.IsNullOrEmpty(gameConfig.PlayerName) ? "Player1" : gameConfig.PlayerName;
        Debug.Log("ID: " + playerId);
        Debug.Log("Name: " + playerName);

        // Initialize components
        health = GetComponent<PlayerHealth>();
        Shoot = GetComponent<PlayerShoot>();
        Movement = GetComponent<FPSController>();

        // Verify player has a PlayerHealth component
        if (health == null)
            health = gameObject.AddComponent<PlayerHealth>();

        health.SetHealth(maxHealth);

    }

    private void Update()
    {
        if (!CheckIfDead())
        {
            if (!isRespawning)
            {
                Movement.HandleMovement();
            }
            Shoot.HandleShooting();
        }

        // DEBUG INPUTS
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(100);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            Heal(10);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Current Health: " + health.GetCurrentHealth().ToString());
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetAtRespawn();
        }
    }

    public void TakeDamage(float damage)
    {
        health.TakeDamage(damage);
    }

    public void Heal(float amount)
    {
        health.Heal(amount);
    }

    public int GetEnergy()
    {
        return currentEnergy;
    }

    public int GetAmmoCount()
    {
        return Shoot.currentAmmo;
    }

    public bool CheckIfDead()
    {
        if (health.isDead) return true;
        else return false;
    }

    public void SetAtRespawn()
    {
        isRespawning = true;

        transform.position = gameConfig.RespawnPos;
        transform.rotation = gameConfig.RespawnRot;

        StartCoroutine(CompleteRespawn());
    }

    private IEnumerator CompleteRespawn()
    {
        yield return new WaitForSeconds(0.1f);
        isRespawning = false;
    }
}
