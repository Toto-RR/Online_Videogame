using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyDisplayUI : MonoBehaviour
{
    public Player player;
    public GameObject energyIconPrefab;
    public Transform energyBarContainer;

    private List<GameObject> energyIcons = new List<GameObject>();

    private void Start()
    {
        UpdateEnergyBar();
    }

    private void Update()
    {
        if (energyIcons.Count != player.GetEnergy())
        {
            UpdateEnergyBar();
        }
    }

    private void UpdateEnergyBar()
    {
        foreach (GameObject icon in energyIcons)
        {
            Destroy(icon);
        }
        energyIcons.Clear();

        for (int i = 0; i < player.GetEnergy(); i++)
        {
            GameObject newIcon = Instantiate(energyIconPrefab, energyBarContainer);
            energyIcons.Add(newIcon);
        }
    }
}
