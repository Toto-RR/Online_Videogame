using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyDisplayUI : MonoBehaviour
{
    public Player player; // Referencia al script del jugador
    public GameObject energyIconPrefab; // Prefab de la imagen de energía
    public Transform energyBarContainer; // Contenedor para las imágenes de energía

    private List<GameObject> energyIcons = new List<GameObject>();

    private void Start()
    {
        UpdateEnergyBar();
    }

    private void Update()
    {
        // Actualiza la barra de energía si cambió el valor
        if (energyIcons.Count != player.GetEnergy())
        {
            UpdateEnergyBar();
        }
    }

    private void UpdateEnergyBar()
    {
        // Limpia las imágenes actuales
        foreach (GameObject icon in energyIcons)
        {
            Destroy(icon);
        }
        energyIcons.Clear();

        // Genera nuevas imágenes según la energía actual
        for (int i = 0; i < player.GetEnergy(); i++)
        {
            GameObject newIcon = Instantiate(energyIconPrefab, energyBarContainer);
            energyIcons.Add(newIcon);
        }
    }
}
