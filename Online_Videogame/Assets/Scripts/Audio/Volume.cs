using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Scrollbar volumeScrollbar; // Referencia al Scrollbar del UI
    public AudioSource musicSource; // Referencia al AudioSource de la música
    public AudioSource backgroundSource; // Referencia opcional al AudioSource de los sonidos de fondo

    public Button soundButton; // El botón que sirve para silenciar/restaurar el sonido
    public Sprite soundOnIcon; // Ícono para sonido activado
    public Sprite soundOffIcon; // Ícono para sonido desactivado

    private float previousVolume = 1f; // Para almacenar el volumen anterior antes de silenciar
    private bool isMuted = false; // Estado del mute

    void OnEnable()
    {
        // Inicializar el volumen basado en la posición actual del Scrollbar
        SetVolume(volumeScrollbar.value);

        // Suscribir el método SetVolume al evento de cambio del Scrollbar
        volumeScrollbar.onValueChanged.AddListener(SetVolume);

        // Configurar el botón para silenciar/restaurar sonido
        soundButton.onClick.AddListener(ToggleMute);

        // Establecer el ícono inicial
        UpdateSoundIcon();
    }

    void OnDisable()
    {
        // Desuscribir los eventos para evitar posibles problemas cuando el canvas está desactivado
        volumeScrollbar.onValueChanged.RemoveListener(SetVolume);
        soundButton.onClick.RemoveListener(ToggleMute);
    }

    // Método que ajusta el volumen de los AudioSources
    public void SetVolume(float value)
    {
        // Ajustar el volumen del musicSource al valor del Scrollbar
        if (musicSource != null)
        {
            musicSource.volume = value;
        }

        // Ajustar el volumen del backgroundSource al valor del Scrollbar si existe
        if (backgroundSource != null)
        {
            backgroundSource.volume = value;
        }

        // Guardar el valor actual del volumen
        previousVolume = value;

        // Verificar si el volumen es 0
        if (value == 0)
        {
            isMuted = true; // Consideramos que el volumen en 0 es equivalente a estar en "Mute"
        }
        else
        {
            isMuted = false; // No está silenciado si el volumen es mayor que 0
        }

        // Actualizar el ícono basado en el valor del volumen
        UpdateSoundIcon();
    }

    // Método para cambiar entre silenciar y restaurar sonido
    public void ToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            // Si el juego está silenciado, ponemos el volumen a 0
            if (musicSource != null) musicSource.volume = 0;
            if (backgroundSource != null) backgroundSource.volume = 0;

            // Actualizar el valor del scrollbar a 0 visualmente
            volumeScrollbar.value = 0;
        }
        else
        {
            // Si el juego se desmutea, restauramos el volumen anterior
            if (musicSource != null) musicSource.volume = previousVolume;
            if (backgroundSource != null) backgroundSource.volume = previousVolume;

            // Actualizar el valor del scrollbar al valor anterior
            volumeScrollbar.value = previousVolume;
        }

        // Actualizar el ícono del botón de sonido
        UpdateSoundIcon();
    }

    // Método para actualizar el ícono dependiendo del estado
    private void UpdateSoundIcon()
    {
        if (isMuted || volumeScrollbar.value == 0)
        {
            soundButton.image.sprite = soundOffIcon;
        }
        else
        {
            soundButton.image.sprite = soundOnIcon;
        }
    }
}
