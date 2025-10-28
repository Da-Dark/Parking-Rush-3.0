using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelCounterManager : MonoBehaviour
{
    public static LevelCounterManager Instance;

    [Header("UI")]
    public TextMeshProUGUI levelCounterText;  // Assign in Inspector if possible

    private int levelCount = 1; // Start at Level 1

    private void Awake()
    {
        // Singleton pattern to persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Try to find UI when scene loads
        SceneManager.sceneLoaded += (scene, mode) => FindTextObject();
    }

    private void Start()
    {
        if (levelCounterText == null)
            FindTextObject();

        UpdateLevelUI();
    }

    private void FindTextObject()
    {
        // Auto-find any TextMeshProUGUI in the scene
        if (levelCounterText == null)
        {
            levelCounterText = FindObjectOfType<TextMeshProUGUI>();
        }

        if (levelCounterText != null)
        {
            UpdateLevelUI();
        }
        else
        {
            Debug.LogWarning("⚠️ LevelCounterManager: Could not find a TextMeshProUGUI in this scene!");
        }
    }

    /// <summary>
    /// Called when the player completes a level successfully.
    /// </summary>
    public void AddLevel()
    {
        levelCount++;
        UpdateLevelUI();
        Debug.Log("📈 Level increased to " + levelCount);
    }

    /// <summary>
    /// Reset the level counter to Level 1 (called when restarting).
    /// </summary>
    public void ResetCounter()
    {
        levelCount = 1;
        UpdateLevelUI();
        Debug.Log("🔄 Level counter reset to 1");
    }

    /// <summary>
    /// Returns the current level number for other scripts (like Deathscreen).
    /// </summary>
    public int GetCurrentLevel()
    {
        return levelCount;
    }

    /// <summary>
    /// Update the level number displayed on-screen.
    /// </summary>
    private void UpdateLevelUI()
    {
        if (levelCounterText != null)
        {
            levelCounterText.text = "Level: " + levelCount;
        }
        else
        {
            Debug.LogWarning("⚠️ LevelCounterManager: levelCounterText not assigned in Inspector!");
        }
    }
}