using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class Deathscreen : MonoBehaviour
{
    //add something to update the level conuter ui
    public TextMeshProUGUI levelReachedText; // assign in inspector

    [Header("Buttons")]
    private Button RestartButton;
    private Button QuitButton;


    private void Awake()
    {
        // Auto-find buttons inside this object
        RestartButton = transform.Find("RestartButton").GetComponent<Button>();
        QuitButton = transform.Find("QuitButton").GetComponent<Button>();
    }

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