using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Deathscreen : MonoBehaviour
{
    public TextMeshProUGUI levelReachedText; // assign in inspector

    private void OnEnable()
    {
        if (LevelCounterManager.Instance != null && levelReachedText != null)
        {
            int currentLevel = LevelCounterManager.Instance.GetCurrentLevel();
            levelReachedText.text = "You reached Level " + currentLevel + "!";
        }
    }

    public void OnRestartButton()
    {
        if (LevelCounterManager.Instance != null)
            LevelCounterManager.Instance.ResetCounter();

        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}