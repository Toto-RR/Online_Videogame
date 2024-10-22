using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Scrollbar volumeScrollbar; // Referencia al Scrollbar del UI
    public AudioSource musicSource; // Referencia al AudioSource de la m�sica
    public AudioSource backgroundSource; // Referencia opcional al AudioSource de los sonidos de fondo

    public Button soundButton; // El bot�n que sirve para silenciar/restaurar el sonido
    public Sprite soundOnIcon; // �cono para sonido activado
    public Sprite soundOffIcon; // �cono para sonido desactivado

    private float previousVolume = 1f; // Para almacenar el volumen anterior antes de silenciar
    private bool isMuted = false; // Estado del mute

    void OnEnable()
    {
        // Inicializar el volumen basado en la posici�n actual del Scrollbar
        SetVolume(volumeScrollbar.value);

        // Suscribir el m�todo SetVolume al evento de cambio del Scrollbar
        volumeScrollbar.onValueChanged.AddListener(SetVolume);

        // Configurar el bot�n para silenciar/restaurar sonido
        soundButton.onClick.AddListener(ToggleMute);

        // Establecer el �cono inicial
        UpdateSoundIcon();
    }

    void OnDisable()
    {
        // Desuscribir los eventos para evitar posibles problemas cuando el canvas est� desactivado
        volumeScrollbar.onValueChanged.RemoveListener(SetVolume);
        soundButton.onClick.RemoveListener(ToggleMute);
    }

    // M�todo que ajusta el volumen de los AudioSources
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
            isMuted = false; // No est� silenciado si el volumen es mayor que 0
        }

        // Actualizar el �cono basado en el valor del volumen
        UpdateSoundIcon();
    }

    // M�todo para cambiar entre silenciar y restaurar sonido
    public void ToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            // Si el juego est� silenciado, ponemos el volumen a 0
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

        // Actualizar el �cono del bot�n de sonido
        UpdateSoundIcon();
    }

    // M�todo para actualizar el �cono dependiendo del estado
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
