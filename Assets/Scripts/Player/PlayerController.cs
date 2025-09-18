using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f; // опційно: Space для стрибка

    [Header("Mouse Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 120f;
    public float minPitch = -75f;
    public float maxPitch = 75f;

    private CharacterController controller;
    private Vector3 velocity;
    private float pitch = 0f;

    // анімації (опційно)
    private Animator animator;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>(); // якщо Animator на дочірньому об'єкті
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Обертаємо корпус вліво/вправо
        transform.Rotate(Vector3.up * mouseX);

        // Нахил камери вгору/вниз (pitch)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");   // A/D
        float inputZ = Input.GetAxisRaw("Vertical");     // W/S
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 move = transform.right * inputX + transform.forward * inputZ;
        move = move.normalized;

        float speed = isRunning ? runSpeed : walkSpeed;

        // Застосувати рух по землі
        Vector3 horizontalVelocity = move * speed;

        // Гравітація і стрибок
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        // Підсумковий рух: горизонтальний + вертикальний
        Vector3 finalMove = horizontalVelocity + new Vector3(0, velocity.y, 0);
        controller.Move(finalMove * Time.deltaTime);

        // Оновити анімації (якщо є Animator і параметри)
        if (animator != null)
        {
            float speed01 = new Vector2(horizontalVelocity.x, horizontalVelocity.z).magnitude;
            animator.SetFloat("Speed", speed01);     // 0..runSpeed
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsGrounded", controller.isGrounded);
        }
    }
}
