using UnityEngine;
using TMPro;

public class AmmoDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI ammoText;
    public Player player;

    void Start()
    {
        if (ammoText != null)
        {
            UpdateAmmoText();
        }
        else
        {
            Debug.LogError("TextMeshProUGUI not found in the scene!");
        }
    }

    void Update()
    {
        if (ammoText != null && player != null)
        {
            UpdateAmmoText();
        }
    }

    private void UpdateAmmoText()
    {
        if (ammoText != null && player != null)
        {
            ammoText.text = "Ammo: " + player.GetAmmoCount().ToString() + " / " + player.Shoot.maxAmmo;
        }
    }
}
