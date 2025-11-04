using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Needed for UI elements

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public Text quitButtonText; // Assign the Quit button's Text component in the Inspector

    private void OnEnable()
    {
        // Pause the game when menu becomes active
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        // Resume the game when menu is closed
        Time.timeScale = 1f;
    }

    // Called by Resume button
    public void OnResumeButton()
    {
        gameObject.SetActive(false); // This triggers OnDisable() and resumes the game
    }

    // Called by Restart button
    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Called by Quit button
    public void OnQuitButton()
    {
        // Reset the Quit button text before returning to the main menu
        if (quitButtonText != null)
        {
            quitButtonText.text = "Quit"; // Replace with whatever default text you want
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Main menu
    }
}
