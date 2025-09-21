using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    [Header("Mouse Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 1.0f; // множник для чутливості Input System
    public float minPitch = -75f;
    public float maxPitch = 75f;

    [Header("Input")]
    public InputActionReference moveAction; // Vector2
    public InputActionReference lookAction; // Vector2
    public InputActionReference jumpAction; // Button
    public InputActionReference runAction;  // Button

    private CharacterController controller;
    private Vector3 velocity;
    private float pitch = 0f;
    private Animator animator;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        moveAction?.action.Enable();
        lookAction?.action.Enable();
        jumpAction?.action.Enable();
        runAction?.action.Enable();
    }

    void OnDisable()
    {
        moveAction?.action.Disable();
        lookAction?.action.Disable();
        jumpAction?.action.Disable();
        runAction?.action.Disable();
    }

    void Update()
    {
        Debug.Log("Скрипт працює! Час: " + Time.time); // <-- ДОДАЙ ЦЕЙ РЯДОК

        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        Vector2 look = lookAction != null ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;

        // Обертання корпусу по горизонталі (yaw)
        transform.Rotate(Vector3.up * look.x * mouseSensitivity);

        // Нахил камери (pitch)
        pitch -= look.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        Vector2 input = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        bool isRunning = runAction != null && runAction.action.IsPressed();

        Vector3 moveDir = transform.right * input.x + transform.forward * input.y;
        moveDir = moveDir.normalized;

        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 horizontalVelocity = moveDir * speed;

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (jumpAction != null && jumpAction.action.triggered && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = horizontalVelocity + new Vector3(0, velocity.y, 0);
        controller.Move(finalMove * Time.deltaTime);

        if (animator != null)
        {
            float speed01 = new Vector2(horizontalVelocity.x, horizontalVelocity.z).magnitude;
            animator.SetFloat("Speed", speed01);
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsGrounded", controller.isGrounded);
        }
    }
}

