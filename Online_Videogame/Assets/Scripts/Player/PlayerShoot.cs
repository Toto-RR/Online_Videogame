using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public LineRenderer raycastLine; // Línea para dibujar el raycast
    public GameObject shootEffectPrefab; // Prefab del efecto visual de disparo

    // Manejo del disparo
    internal void HandleShooting(float damageAmount, float shootRange)
    {
        if (Input.GetMouseButtonDown(0)) // Left Click
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(ray, out hit, shootRange))
            {
                GameObject hitObject = hit.collider.gameObject;
                string targetPlayerId = GetTargetPlayerId(hitObject);

                if (targetPlayerId != null)
                {
                    SendDamage(damageAmount, targetPlayerId);

                    // Create an effect to shoot if exists
                    if (shootEffectPrefab != null)
                    {
                        Instantiate(shootEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    }
                }
                else
                {
                    Debug.Log("The object hitted doesn't have PlayerIdentity");
                }
            }
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
}
