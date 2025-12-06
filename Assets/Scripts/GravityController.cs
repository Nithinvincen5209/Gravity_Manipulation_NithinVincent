using UnityEngine;
using System.Collections;

public class GravityController : MonoBehaviour
{
    [Header("Components")]
    public Transform playerModel;
    public Transform hologram;
    public Transform camTransform;
    public Animator animator;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    public float gravityAmount = 20f;
    public float adjustSpeed = 5f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2.0f;
    public float lookUpLimit = 80f;

    [Header("Fixes (Check these if controls feel wrong)")]
    [Tooltip("Check this if W moves you Backward")]
    public bool invertMovement = false;

    [Tooltip("Check this if Mouse UP makes you look DOWN")]
    public bool invertMouse = false;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float rayLength = 3.0f;

    private Rigidbody rb;
    private Vector3 gravityDirection = Vector3.down;
    private Vector3 moveInput;
    private bool isGrounded;
    private float cameraVerticalAngle = 0f;

    // Gravity Flip Logic
    private bool isSelectingGravity = false;
    private bool isFlipping = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (hologram) hologram.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isFlipping) return;

        CheckGround();
        HandleMouseLook();
        HandleMovementInput(); // WASD Only
        HandleGravitySelection(); // Arrows Only
        UpdateAnimations();

        if (isSelectingGravity && Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(FlipGravityRoutine());
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(gravityDirection * gravityAmount, ForceMode.Acceleration);

        // Movement Logic
        float directionMultiplier = invertMovement ? -1f : 1f;

        // Move relative to Player Facing (Local Forward/Right)
        Vector3 targetVelocity = (transform.forward * moveInput.z + transform.right * moveInput.x) * moveSpeed * directionMultiplier;

        float verticalVel = Vector3.Dot(rb.velocity, gravityDirection);
        Vector3 newVelocity = targetVelocity + (gravityDirection * verticalVel);

        rb.velocity = Vector3.Lerp(rb.velocity, newVelocity, 15 * Time.fixedDeltaTime);

        // Align Upright
        if (!isFlipping)
        {
            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * currentRotation;
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, adjustSpeed * Time.fixedDeltaTime);
        }
    }

    void HandleMouseLook()
    {
        // 1. Body Rotation (Left/Right)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.RotateAround(transform.position, -gravityDirection, mouseX);

        // 2. Camera Rotation (Up/Down)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        if (invertMouse) cameraVerticalAngle += mouseY;
        else cameraVerticalAngle -= mouseY;

        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -lookUpLimit, lookUpLimit);

        if (camTransform != null)
        {
            camTransform.localRotation = Quaternion.Euler(cameraVerticalAngle, 0f, 0f);
        }
    }

    void HandleMovementInput()
    {
        // --- WASD Movement ---
        float x = 0;
        float z = 0;

        if (Input.GetKey(KeyCode.W)) z = 1;
        if (Input.GetKey(KeyCode.S)) z = -1;
        if (Input.GetKey(KeyCode.D)) x = 1;
        if (Input.GetKey(KeyCode.A)) x = -1;

        moveInput = new Vector3(x, 0, z);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Vector3 velocityProjectedOnGravity = Vector3.Project(rb.velocity, gravityDirection);
            rb.velocity = rb.velocity - velocityProjectedOnGravity;
            rb.AddForce(-gravityDirection * jumpForce, ForceMode.Impulse);
        }
    }
    void HandleGravitySelection()
    {
        // --- ARROWS ONLY ---
        bool up = Input.GetKeyDown(KeyCode.UpArrow);
        bool down = Input.GetKeyDown(KeyCode.DownArrow);
        bool left = Input.GetKeyDown(KeyCode.LeftArrow);
        bool right = Input.GetKeyDown(KeyCode.RightArrow);

        if (up || down || left || right)
        {
            // Reset Hologram to match Player first (Snap, don't spin)
            isSelectingGravity = true;
            hologram.gameObject.SetActive(true);
            hologram.rotation = transform.rotation;

            // --- FEET DIRECTION LOGIC ---

            // Left Arrow = Feet point LEFT (Rotate Head to Right)
            if (left) hologram.Rotate(0, 0, -90, Space.Self);

            // Right Arrow = Feet point RIGHT (Rotate Head to Left)
            if (right) hologram.Rotate(0, 0, 90, Space.Self);

            // Up Arrow = Feet point FORWARD (Rotate Head Back)
            if (up) hologram.Rotate(-90, 0, 0, Space.Self);

            // Down Arrow = Feet point BACKWARD (Rotate Head Forward)
            if (down) hologram.Rotate(90, 0, 0, Space.Self);
        }

        // Cancel Selection
        if (Input.GetKeyDown(KeyCode.Escape) && isSelectingGravity)
        {
            isSelectingGravity = false;
            hologram.gameObject.SetActive(false);
        }
    }
    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("isGrounded", isGrounded);
            bool isMoving = moveInput.magnitude > 0.1f;
            animator.SetBool("isRunning", isMoving);
        }
    }

    IEnumerator FlipGravityRoutine()
    {
        isFlipping = true;
        isSelectingGravity = false;
        hologram.gameObject.SetActive(false);

        Vector3 newGravityDir = -hologram.up;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = hologram.rotation;

        rb.AddForce(-gravityDirection * 2f, ForceMode.Impulse);

        float time = 0;
        float duration = 0.5f;

        while (time < duration)
        {
            transform.rotation = Quaternion.Slerp(startRot, endRot, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRot;
        gravityDirection = newGravityDir;
        isFlipping = false;
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, gravityDirection, rayLength, groundLayer);
        Debug.DrawRay(transform.position, gravityDirection * rayLength, isGrounded ? Color.green : Color.red);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Trap"))
        {
            GameManager.Instance.GameOver();
        }
    }
}