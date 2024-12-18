using System.Collections;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public LineRenderer raycastLine; // Line to draw raycast
    public GameObject shootEffectPrefab; // Prefab for a shoot VFX

    public Camera playerCamera;
    public RectTransform crosshair; // "Crosshair" reference
    public HitmarkerUI hitmarker; // Hitmarker reference

    public int maxAmmo = 30;
    public int currentAmmo;
    public float damage = 10f;
    public float shootRange = 50f;
    public float reloadTime = 2f;

    private bool isReloading = false;

    // Audio variables
    public AudioClip shootSound; // Sound for shooting
    public AudioClip reloadSound; // Sound for reloading
    private AudioSource audioSource;

    void Start()
    {
        currentAmmo = maxAmmo;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    internal void HandleShooting()
    {
        // If its not reloading and has ammo
        if (currentAmmo > 0 && !isReloading && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = CreateShootRay();

            if (Physics.Raycast(ray, out hit, shootRange))
            {
                GameObject hitObject = hit.collider.gameObject;
                string targetPlayerId = GetTargetPlayerId(hitObject);

                if (targetPlayerId != null)
                {
                    hitmarker.GetHitmarker();
                    SendDamage(damage, targetPlayerId);
                }

                if (shootEffectPrefab != null)
                {
                    Instantiate(shootEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                }
            }

            currentAmmo--;

            // Play shooting sound
            PlaySound(shootSound);
        }
        else if (Input.GetKeyDown(KeyCode.R) && !isReloading) // Reload when R
        {
            StartCoroutine(Reload());
        }
    }

    private void SendDamage(float damageAmount, string targetPlayerId)
    {
        PlayerSync.Instance.HandleShooting(damageAmount, targetPlayerId);
    }

    private string GetTargetPlayerId(GameObject hitObject)
    {
        return hitObject.GetComponent<PlayerIdentity>()?.PlayerId;
    }

    private Ray CreateShootRay()
    {
        return playerCamera.ScreenPointToRay(crosshair.position);
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        // Play reload sound
        PlaySound(reloadSound);

        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
