using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public AudioSource audioSource; // El AudioSource que reproducirá el sonido
    public AudioClip soundClip; // El clip de audio a reproducir

    // Esta función se llamará desde el evento de la animación
    public void PlaySound()
    {
        if (audioSource != null && soundClip != null)
        {
            audioSource.PlayOneShot(soundClip);
        }
        else
        {
            Debug.LogWarning("AudioSource o AudioClip no asignado.");
        }
    }
}