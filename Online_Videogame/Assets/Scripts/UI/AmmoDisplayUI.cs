using UnityEngine;
using TMPro; // Importar para TextMeshPro

public class AmmoDisplayUI : MonoBehaviour
{
    private TextMeshProUGUI ammoText; // Referencia al objeto TextMeshProUGUI
    public Player player; // Referencia al script Player para obtener las balas

    void Start()
    {
        // Intentar encontrar autom�ticamente el componente TextMeshProUGUI dentro de la jerarqu�a
        ammoText = FindObjectOfType<TextMeshProUGUI>();

        // Asegurarse de que el texto est� inicializado y actualizado al inicio
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
        // Actualizar el texto de las balas en cada frame
        if (ammoText != null && player != null)
        {
            UpdateAmmoText();
        }
    }

    private void UpdateAmmoText()
    {
        // Verifica que el jugador tenga un valor v�lido de balas
        if (ammoText != null && player != null)
        {
            ammoText.text = "Ammo: " + player.GetAmmoCount().ToString();
        }
    }
}
