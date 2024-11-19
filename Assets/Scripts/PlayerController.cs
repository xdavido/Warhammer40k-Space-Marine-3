using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField]
    private float rotationSpeed = 5f;


    private CharacterController controller;
    private PlayerInput playerInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private Transform cameraTransform;

    [SerializeField]
    private GameObject screenPlayer =null;


    [SerializeField]
    private Transform camera = null;


    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;
    private InputAction jumpAction;


    bool movementBlocked = false;
    GameState gameState;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Shoot"];
        jumpAction = playerInput.actions["Jump"];
    }
    private void Start()
    {
        
        playerInput = GetComponent<PlayerInput>();
        gameState = FindObjectOfType<GameState>();
        cameraTransform = camera;


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
        if(Physics.Raycast(cameraTransform.position,cameraTransform.forward,out hit, Mathf.Infinity))
        {

        }
    }

    void Update()
    {

        if (movementBlocked || gameState.isGamePaused) return;

        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0, input.y);
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0;
        controller.Move(move * Time.deltaTime * playerSpeed);



        // Makes the player jump
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        
       

        Quaternion rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        float angleDifference = Quaternion.Angle(transform.rotation, rotation);
        float adaptiveSpeed = rotationSpeed * (angleDifference / 180f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, adaptiveSpeed * Time.deltaTime);
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
}
