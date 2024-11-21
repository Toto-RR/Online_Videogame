using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyDisplayUI : MonoBehaviour
{
    public Player player; // Referencia al script del jugador
    public GameObject energyIconPrefab; // Prefab de la imagen de energ�a
    public Transform energyBarContainer; // Contenedor para las im�genes de energ�a

    private List<GameObject> energyIcons = new List<GameObject>();

    private void Start()
    {
        UpdateEnergyBar();
    }

    private void Update()
    {
        // Actualiza la barra de energ�a si cambi� el valor
        if (energyIcons.Count != player.GetEnergy())
        {
            UpdateEnergyBar();
        }
    }

    private void UpdateEnergyBar()
    {
        // Limpia las im�genes actuales
        foreach (GameObject icon in energyIcons)
        {
            Destroy(icon);
        }
        energyIcons.Clear();

        // Genera nuevas im�genes seg�n la energ�a actual
        for (int i = 0; i < player.GetEnergy(); i++)
        {
            GameObject newIcon = Instantiate(energyIconPrefab, energyBarContainer);
            energyIcons.Add(newIcon);
        }
    }
}
