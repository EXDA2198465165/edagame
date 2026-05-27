using UnityEngine;

/// <summary>
/// Controls player movement and interaction in third-person view
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float airControl = 0.4f;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDragConstant = 5f;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 45f;

    private Rigidbody rb;
    private CameraController cameraController;

    private Vector3 moveDirection;
    private float horizontalInput;
    private float verticalInput;
    private float currentSpeed;
    private bool isGrounded;
    private bool readyToJump = true;
    private RaycastHit slopeHit;
    private bool onSlope;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraController = GetComponentInChildren<CameraController>();

        // Freeze rotation to prevent tipping over
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // Ground check using raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 
            playerHeight * 0.5f + 0.2f, groundLayer);

        HandleInput();
        HandleSpeedControl();
        HandleDrag();

        // Allow jump input
        if (Input.GetKeyDown(KeyCode.Space) && readyToJump && isGrounded)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInput()
    {
        // Get movement input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Determine if sprinting
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
    }

    private void MovePlayer()
    {
        // Calculate movement direction relative to camera
        moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        // Apply movement
        if (isGrounded && !onSlope)
        {
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);
        }
        else if (isGrounded && onSlope)
        {
            rb.AddForce(GetSlopeMovementDirection(moveDirection) * currentSpeed * 10f, ForceMode.Force);
        }
        else if (!isGrounded)
        {
            // Reduced air control
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f * airControl, ForceMode.Force);
        }

        // Rotate player to face movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void HandleSpeedControl()
    {
        // Limit velocity on ground
        if (isGrounded)
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > currentSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * currentSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void HandleDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void Jump()
    {
        // Reset Y velocity before jumping
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Add jump force
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        // Jump cooldown
        readyToJump = false;
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            onSlope = angle < maxSlopeAngle && angle != 0;
        }
    }

    private Vector3 GetSlopeMovementDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    /// <summary>
    /// Returns the current velocity of the player
    /// </summary>
    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    /// <summary>
    /// Checks if the player is currently grounded
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }
}
