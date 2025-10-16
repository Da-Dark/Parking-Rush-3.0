using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Deathscreen : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelReachedText; // Drag your Text object here

    public void ShowDeathScreen()
    {
        // Display the current level reached
        if (LevelCounterManager.Instance != null && levelReachedText != null)
        {
            levelReachedText.text = "You reached Level: " + LevelCounterManager.Instance.GetCurrentLevel();
        }

        gameObject.SetActive(true);
        Time.timeScale = 0; // Pause the game
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1;

        // Reset Level Counter
        LevelCounterManager.Instance?.ResetCounter();

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
