using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages button inputs for the Main Menu and UI Canvas screens (Game Over, Win, etc.).
/// Handles scene transitions and application quitting.
/// </summary>
public class GameUI : MonoBehaviour
{
    /// <summary>
    /// Reloads the currently active scene.
    /// Used for "Try Again" or "Restart" buttons.
    /// </summary>
    public void RestartButton()
    {
        // Ensure time is running before reloading
        Time.timeScale = 1f;

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Loads the Main Menu scene (Index 0).
    /// </summary>
    public void HomeButton()
    {
        // Ensure time is running
        Time.timeScale = 1f;

        Debug.Log("Loading Home Scene");
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Loads the first Level (Index 1).
    /// Used on the Main Menu "Start" button.
    /// </summary>
    public void StartButton()
    {
        // Ensure time is running
        Time.timeScale = 1f;

        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Closes the application.
    /// Works in the built standalone application (Windows/Mac).
    /// </summary>
    public void QuitButton()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}