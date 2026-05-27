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
