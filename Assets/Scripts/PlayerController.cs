using System.Collections;
using System.Collections.Generic;
using _MessageType;
using UnityEngine;

using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // ===========================================================
    // Movement Settings
    // ===========================================================

    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 2.0f;           // Player movement speed
    [SerializeField] private float jumpHeight = 1.0f;            // Jump height
    [SerializeField] private float gravityValue = -9.81f;        // Gravity applied to the player
    [SerializeField] private float rotationSpeed = 5f;           // Player rotation speed

    // ===========================================================
    // Dash Settings
    // ===========================================================

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5.0f;          // Dash distance
    [SerializeField] private float dashDuration = 0.2f;          // Dash duration
    [SerializeField] private float dashCooldown = 1.0f;          // Time between dashes
    private float dashTime = 0f;
    private bool isDashing = false;
    private bool canDash = true;
    private Vector3 dashDirection;

    // ===========================================================
    // Camera and Effects Settings
    // ===========================================================

    [Header("Camera and Effects Settings")]
    [SerializeField] private Cinemachine.CinemachineImpulseSource impulseSource; // Impulse for camera shake
    [SerializeField] private float shakeDuration = 0.2f;         // Shake duration
    [SerializeField] private float shakeMagnitude = 0.1f;        // Shake magnitude
    [SerializeField] private Transform cameras = null;           // Reference to the player's camera

    // ===========================================================
    // Gameplay Settings
    // ===========================================================

    [Header("Gameplay Settings")]
    [SerializeField] private float groundCheckDistance = 1f;      // Distance to check if the player is grounded
    [SerializeField] private LayerMask groundLayer;              // Ground layer for checking ground collisions

    // ===========================================================
    // Component References
    // ===========================================================

    [Header("Component References")]
    private CharacterController controller;
    private PlayerInput playerInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private Transform cameraTransform;
    private GameState gameState;

    // ===========================================================
    // Input Actions
    // ===========================================================

    [Header("Input Actions")]
    private InputAction moveAction;      // Player movement action
    private InputAction lookAction;      // Camera look action
    private InputAction shootAction;     // Shooting action
    private InputAction jumpAction;      // Jump action
    private InputAction dashAction;      // Dash action

    // ===========================================================
    // Player State
    // ===========================================================

    [Header("Player State")]
    [SerializeField] private GameObject screenPlayer = null;      // Reference to the player's screen (UI)
    private int playerId;                                         // Player unique ID
    public bool movementBlocked = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Shoot"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];

        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Start()
    {
        
        playerInput = GetComponent<PlayerInput>();
        gameState = FindObjectOfType<GameState>();
        cameraTransform = cameras;


    }

    private void OnEnable()
    {
        shootAction.performed += _ => ShootGun();
    }

    private void OnDisable()
    {
        shootAction.performed -= _ => ShootGun();

    }

    private void ShootGun()
    {

        RaycastHit hit;
        // Realiza el raycast desde la cámara hacia adelante
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity))
        {
            Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");

            // Verifica si el objeto golpeado es un jugador
            PlayerController otherPlayer = hit.collider.GetComponent<PlayerController>();
            if (otherPlayer != null && otherPlayer != this) // Evita detectar al propio jugador
            {
                Debug.Log("Hit another player!");

                MessageManager.SendMessage(new Shoot(otherPlayer.GetPlayerId()));
               
            }
            else
            {
                Debug.Log("Hit something else, but not a player.");
            }
        }
        else
        {
            Debug.Log("No hit detected.");
        }
    }

    void Update()
    {

        if (movementBlocked || gameState.isGamePaused) return;

        groundedPlayer = IsGrounded();
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // Obtiene la entrada del jugador (movimiento)
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0, input.y);

        // Hace el movimiento en relación a la cámara
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0;


        controller.Move(move * Time.deltaTime * playerSpeed);

        // Control de salto
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        // Aplica gravedad
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);


        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDmg();
        }


        if (dashAction.triggered && canDash)
        {
            StartDash(move);
        }

        if (isDashing)
        {
            HandleDash();
        }


        //float angleDifference = Quaternion.Angle(transform.rotation, rotation);
        //float adaptiveSpeed = rotationSpeed * (angleDifference / 180f);
        //transform.rotation = Quaternion.Lerp(transform.rotation, rotation, adaptiveSpeed * Time.deltaTime);


        // **Alineación del jugador con la cámara**
        AlignPlayerWithCamera();
    }

    void AlignPlayerWithCamera()
    {
        // Calcula la dirección hacia donde está mirando la cámara (sin componente Y)
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0; // Ignorar inclinaciones verticales de la cámara

        if (cameraForward.sqrMagnitude > 0.001f) // Asegúrate de que haya una dirección válida
        {
            // Calcula la rotación hacia la dirección de la cámara
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);

            // Verifica si la diferencia de rotación es significativa
            float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
            if (angleDifference > 1f) // Ajusta el umbral si es necesario
            {
                // Rota suavemente hacia la rotación objetivo
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    public void BlockMovement()
    {
        movementBlocked = true;
        screenPlayer.SetActive(false);

    }

    void SetCameraPriority(GameObject player, int priority)
    {
        Cinemachine.CinemachineVirtualCamera virtualCamera = player.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            virtualCamera.Priority = priority;
        }
        else
        {
            Debug.LogWarning($"No se encontró una cámara virtual en el jugador {player.name}");
        }
    }

    public int GetPlayerId()
    {
        return playerId;
    }

    public void SetPlayerId(int id)
    {
        playerId = id;
    }


    public void TakeDmg()
    {
        Debug.Log($"{gameObject.name} has taken damage!");

        impulseSource.GenerateImpulse();
    }


    private IEnumerator ShakeCamera()
    {
        Vector3 originalPosition = cameraTransform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            cameraTransform.localPosition = new Vector3(
                originalPosition.x + offsetX,
                originalPosition.y + offsetY,
                originalPosition.z
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset camera to its original position
        cameraTransform.localPosition = originalPosition;
    }

    private bool IsGrounded()
    {
        RaycastHit hit;
        return Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer);
    }

    private void StartDash(Vector3 direction)
    {
        // Comenzamos el dash
        isDashing = true;
        canDash = false; // No se puede hacer otro dash hasta que termine el cooldown
        dashTime = 0f; // Reiniciar el tiempo del dash

        // Si el jugador se está moviendo, el dash será en la dirección del movimiento
        

        // Si el jugador no se está moviendo, entonces el dash será hacia adelante (dirección de la cámara)
        if (direction.sqrMagnitude < 0.1f)
        {
            direction = cameraTransform.forward.normalized;
        }

        dashDirection = direction;

        // Inicia el cooldown del dash
        Invoke(nameof(ResetDashCooldown), dashCooldown);
    }

    private void HandleDash()
    {
        // Realizamos el dash
        if (dashTime < dashDuration)
        {
            controller.Move(dashDirection * dashDistance * Time.deltaTime);
            dashTime += Time.deltaTime;
        }
        else
        {
            // Después de que termine el dash, volvemos a permitir el movimiento normal
            isDashing = false;
        }
    }

    private void ResetDashCooldown()
    {
        canDash = true; // Ya podemos hacer otro dash después del cooldown
    }
}
