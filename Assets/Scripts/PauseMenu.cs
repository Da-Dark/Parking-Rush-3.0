using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Main menu
    }
}
