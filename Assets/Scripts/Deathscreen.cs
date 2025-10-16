using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Deathscreen : MonoBehaviour
{
    public TextMeshProUGUI levelReachedText; // Assign in Inspector

    private void OnEnable()
    {
        // Update the message when the death screen is shown
        if (LevelCounterManager.Instance != null && levelReachedText != null)
        {
            int currentLevel = LevelCounterManager.Instance.GetCurrentLevel();
            levelReachedText.text = "You reached Level " + currentLevel + "!";
        }
    }

    public void OnRestartButton()
    {
        // Reset level counter to 1
        if (LevelCounterManager.Instance != null)
        {
            LevelCounterManager.Instance.ResetCounter();
        }

        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}