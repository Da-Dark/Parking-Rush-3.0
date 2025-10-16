using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelCounterManager : MonoBehaviour
{
    public static LevelCounterManager Instance;

    [Header("UI")]
    public TextMeshProUGUI levelCounterText;  // Drag your TextMeshPro object here

    private int levelCount = 1; // Start at Level 1

    private void Awake()
    {
        // Singleton pattern
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

        // If no Text assigned, try to find one in the scene
        if (levelCounterText == null)
        {
            FindTextObject();
        }

        // Reconnect text whenever a new scene is loaded
        SceneManager.sceneLoaded += (scene, mode) => FindTextObject();
    }

    private void Start()
    {
        UpdateLevelUI();
    }

    private void FindTextObject()
    {
        if (levelCounterText == null)
        {
            levelCounterText = FindObjectOfType<TextMeshProUGUI>();
            if (levelCounterText != null)
            {
                UpdateLevelUI();
            }
            else
            {
                Debug.LogWarning("LevelCounterManager: Could not find any TextMeshProUGUI in the scene!");
            }
        }
    }

    /// <summary>
    /// Call this whenever the player completes a level
    /// </summary>
    public void AddLevel()
    {
        levelCount++;
        UpdateLevelUI();
    }

    /// <summary>
    /// Reset the level counter to Level 1 (used when restarting)
    /// </summary>
    public void ResetCounter()
    {
        levelCount = 1;
        UpdateLevelUI();
    }

    /// <summary>
    /// Returns the current level number (useful for Deathscreen messages)
    /// </summary>
    public int GetCurrentLevel()
    {
        return levelCount;
    }

    /// <summary>
    /// Updates the on-screen UI
    /// </summary>
    private void UpdateLevelUI()
    {
        if (levelCounterText != null)
        {
            levelCounterText.text = "Level: " + levelCount;
        }
        else
        {
            Debug.LogWarning("LevelCounterText is not assigned in the Inspector!");
        }
    }
}
