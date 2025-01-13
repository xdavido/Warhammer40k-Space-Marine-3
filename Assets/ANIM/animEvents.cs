using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animEvents : MonoBehaviour
{
    public GameObject soundobj; // Objeto con el AudioSource
    public AudioClip walksound; // Clip de sonido para los pasos
    public float stepInterval = 1.0f; // Intervalo entre pasos en segundos

    private AudioSource audioSource;
    private float stepTimer = 0f; // Temporizador para controlar el tiempo entre sonidos

    private void Start()
    {
        audioSource = soundobj.GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Verificar si el personaje está en movimiento
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (isMoving)
        {
            // Incrementar el temporizador
            stepTimer += Time.deltaTime;

            // Reproducir el sonido si se ha alcanzado el intervalo
            if (stepTimer >= stepInterval)
            {
                audioSource.PlayOneShot(walksound);
                stepTimer = 0f; // Reiniciar el temporizador
            }
        }
        else
        {
            // Reiniciar el temporizador si el personaje deja de moverse
            stepTimer = 0f;
        }
    }
}
