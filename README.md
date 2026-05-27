
using UnityEngine;

/// <summary>
/// Manages the third-person camera following the player
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Camera Distance")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 15f;

    [Header("Camera Height")]
    [SerializeField] private float height = 2f;
    [SerializeField] private float minHeight = 1f;
    [SerializeField] private float maxHeight = 5f;

    [Header("Camera Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Rotation")]
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private float collisionSmoothness = 10f;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private float currentDistance;
    private Transform player;
    private Camera mainCamera;

    private void Start()
    {
        player = transform.parent;
        mainCamera = GetComponentInChildren<Camera>();
        currentDistance = distance;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleMouseInput();
        HandleZoom();
        HandleCursorLock();
    }

    private void LateUpdate()
    {
        PositionCamera();
    }

    private void HandleMouseInput()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player (Y axis - horizontal)
        rotationY += mouseX;
        player.Rotate(Vector3.up * mouseX);

        // Rotate camera (X axis - vertical)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
    }

    private void HandleZoom()
    {
        // Scroll wheel to zoom in/out
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scroll * 2f;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }

    private void HandleCursorLock()
    {
        // Toggle cursor lock with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked 
                ? CursorLockMode.None 
                : CursorLockMode.Locked;
        }
    }

    private void PositionCamera()
    {
        // Calculate desired camera position
        Vector3 targetPosition = player.position 
            + Vector3.up * height 
            - player.forward * currentDistance;

        // Check for collisions with environment
        Vector3 directionToCamera = (targetPosition - player.position).normalized;
        float distanceToCamera = currentDistance;

        if (Physics.Raycast(player.position + Vector3.up * height, directionToCamera, 
            out RaycastHit hit, currentDistance, collisionLayer))
        {
            distanceToCamera = hit.distance - 0.5f;
        }

        // Adjust target position based on collision
        targetPosition = player.position 
            + Vector3.up * height 
            - player.forward * distanceToCamera;

        // Smoothly move camera to target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, 
            Time.deltaTime * smoothSpeed);

        // Apply rotation
        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    /// <summary>
    /// Get the current camera distance from player
    /// </summary>
    public float GetDistance()
    {
        return currentDistance;
    }

    /// <summary>
    /// Set the camera distance from player
    /// </summary>
    public void SetDistance(float newDistance)
    {
        currentDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }

    /// <summary>
    /// Get the camera's forward direction
    /// </summary>
    public Vector3 GetCameraForward()
    {
        return transform.forward;
    }

    /// <summary>
    /// Get the camera's right direction
    /// </summary>
    public Vector3 GetCameraRight()
    {
        return transform.right;
    }
}

Please confirm you want Copilot to make this change in the EXDA2198465165/edagame repository on the default branch.

Make these code changes?
Assets/Scripts/Managers/GameManager.cs

cs
using UnityEngine;

/// <summary>
/// Main game manager that handles overall game state and initialization
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private bool showDebugInfo = true;
    private PlayerController playerController;
    private bool isPaused = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Find the player in the scene
        playerController = FindObjectOfType<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("PlayerController not found in scene!");
        }

        // Set up game initial state
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        // Display debug info if enabled
        if (showDebugInfo)
        {
            DisplayDebugInfo();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log(isPaused ? "Game Paused" : "Game Resumed");
    }

    private void DisplayDebugInfo()
    {
        if (playerController != null)
        {
            Vector3 velocity = playerController.GetVelocity();
            bool isGrounded = playerController.IsGrounded();

            Debug.Log($"Velocity: {velocity.magnitude:F2} | Grounded: {isGrounded}");
        }
    }

    /// <summary>
    /// Get the player controller instance
    /// </summary>
    public PlayerController GetPlayerController()
    {
        return playerController;
    }

    /// <summary>
    /// Check if the game is currently paused
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
