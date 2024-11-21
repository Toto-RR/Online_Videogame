using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Slider healthBar;
    public void UpdateHealthBar(float health)
    {
        if (healthBar != null)
        {
            healthBar.value = health;
        }
    }
}
