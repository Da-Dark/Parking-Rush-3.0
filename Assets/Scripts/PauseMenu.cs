using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenu; // assign your PauseMenu Canvas/GameObject here

    private bool isPaused = false;

    void Start()
    {
        // Ensure the menu starts inactive
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    void Update()
    {
        // Toggle pause menu when P or Escape is pressed
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (pauseMenu == null) return;

        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    // Button methods
    public void OnResumeButton()
    {
        if (pauseMenu == null) return;

        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuitButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Main menu
    }
}
