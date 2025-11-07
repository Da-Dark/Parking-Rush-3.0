using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelCounterManager : MonoBehaviour
{
    public static LevelCounterManager Instance;

    [Header("UI")]
    [Tooltip("Assign the Level Counter TMP text in gameplay scenes only.")]
    public TextMeshProUGUI levelCounterText;

    private int levelCount = 1; // Start at Level 1
    private int lastSceneIndex;

    private void Awake()
    {
        // Singleton pattern to persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        UpdateLevelUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Detect when a new scene is loaded
        lastSceneIndex = scene.buildIndex;
        UpdateLevelUI();
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
    /// Resets the level counter to Level 1.
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
    }
}
