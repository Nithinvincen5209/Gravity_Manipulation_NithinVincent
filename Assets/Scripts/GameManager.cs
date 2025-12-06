using UnityEngine;
using TMPro;

/// <summary>
/// Manages the core game loop, including the timer, score tracking, 
/// and Win/Loss states. Implements the Singleton pattern.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    #region Game Settings
    [Header("Game Settings")]
    [Tooltip("Total number of boxes required to win.")]
    public int totalBoxes = 9;

    [Tooltip("Time limit in seconds (e.g., 120 = 2 minutes).")]
    public float timeRemaining = 120f;

    [Tooltip("The UI Canvas that appears when the player loses.")]
    public Canvas gameoverCanvas;

    [Tooltip("The UI Canvas that appears when the player wins.")]
    public Canvas winnerCanvas;
    #endregion

    #region UI References
    [Header("UI References")]
    [Tooltip("Text element to display the countdown.")]
    public TextMeshProUGUI timerText;

    [Tooltip("Text element to display current score (0/9).")]
    public TextMeshProUGUI scoreText;
    #endregion

    #region Private State
    private int currentScore = 0;
    private bool gameActive = true;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        // Singleton Pattern: Ensure only one GameManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Ensure time is running at normal speed
        Time.timeScale = 1f;

        // Initialize UI
        UpdateScoreDisplay();
        UpdateTimerDisplay();
        
    }

    void Update()
    {
        if (gameActive)
        {
            HandleTimer();
        }
    }
    #endregion

    #region Logic Methods
    /// <summary>
    /// Counts down the timer and triggers Game Over if time runs out.
    /// </summary>
    void HandleTimer()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else
        {
            timeRemaining = 0;
            GameOver();
        }
    }

    /// <summary>
    /// Called by LightBox objects when the player collects them.
    /// Increases score and checks for the Win condition.
    /// </summary>
    public void CollectBox()
    {
        if (!gameActive) return;

        currentScore++;
        UpdateScoreDisplay();

        // Check Win Condition
        if (currentScore >= totalBoxes)
        {
            WinGame();
        }
    }

    /// <summary>
    /// Handles the Win state: Stops time and shows the Winner UI.
    /// </summary>
    private void WinGame()
    {
        gameActive = false;
        Time.timeScale = 0f; // Freeze the game
        // Unlock and show the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (winnerCanvas) winnerCanvas.gameObject.SetActive(true);

        // Hide HUD elements for a cleaner look
        if (timerText) timerText.enabled = false;
        if (scoreText) scoreText.enabled = false;

        Debug.Log("Player Won!");
    }

    /// <summary>
    /// Handles the Lose state: Stops time and shows the Game Over UI.
    /// Can be called by the Timer or by Traps.
    /// </summary>
    public void GameOver()
    {
        gameActive = false;
        Time.timeScale = 0f; // Freeze the game
      // Unlock and show the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameoverCanvas) gameoverCanvas.gameObject.SetActive(true);

        if (timerText) timerText.enabled = false;
        if (scoreText) scoreText.enabled = false;

        Debug.Log("Game Over!");
    }
    #endregion

    #region UI Updates
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{currentScore} / {totalBoxes}";
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Format time as 00:00
            float minutes = Mathf.FloorToInt(timeRemaining / 60);
            float seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    #endregion
}