using System.Collections;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public LineRenderer raycastLine; // Línea para dibujar el raycast
    public GameObject shootEffectPrefab; // Prefab del efecto visual de disparo

    public Camera playerCamera;
    public RectTransform crosshair; // Mirilla en el Canvas UI

    public int maxAmmo = 30;   // Munición máxima
    public int currentAmmo;    // Munición actual
    public float damage = 10f;   // Daño del disparo
    public float shootRange = 50f;   // Alcance del disparo
    public float reloadTime = 2f;    // Tiempo de recarga en segundos

    private bool isReloading = false; // Si está recargando

    void Start()
    {
        currentAmmo = maxAmmo; // Inicializar la munición
    }

    internal void HandleShooting()
    {
        // Lógica de disparo
        if (currentAmmo > 0 && !isReloading && Input.GetMouseButtonDown(0)) // Si tiene balas
        {
            RaycastHit hit;
            Ray ray = CreateShootRay();

            if (Physics.Raycast(ray, out hit, shootRange))
            {
                GameObject hitObject = hit.collider.gameObject;
                string targetPlayerId = GetTargetPlayerId(hitObject);

                if (targetPlayerId != null)
                {
                    SendDamage(damage, targetPlayerId);
                }

                if (shootEffectPrefab != null)
                {
                    Instantiate(shootEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                }
            }

            currentAmmo--; // Reducir las balas después de disparar
        }
        else if (Input.GetKeyDown(KeyCode.R) && !isReloading) // Si presiona R
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
        return playerCamera.ScreenPointToRay(crosshair.position); // Crear un raycast desde la mirilla
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);  // Simulación de recarga
        currentAmmo = maxAmmo;   // Recargar balas
        isReloading = false;
    }
}
