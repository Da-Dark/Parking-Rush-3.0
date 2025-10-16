using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelCounterManager : MonoBehaviour
{
    public static LevelCounterManager Instance;

    [Header("UI")]
    public TextMeshProUGUI levelCounterText;

    private int levelCount = 1;

    /// <summary>
    /// Returns the current level number
    /// </summary>
    public int GetCurrentLevel()
    {
        return levelCount;
    }


    private void Awake()
    {
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
            if (levelCounterText == null)
                Debug.LogWarning("⚠️ Could not find TextMeshProUGUI for LevelCounter!");
        }

        UpdateLevelUI();
    }

    public void AddLevel()
    {
        levelCount++;
        UpdateLevelUI();
    }

    public void ResetCounter()
    {
        levelCount = 1;
        UpdateLevelUI();
    }

    private void UpdateLevelUI()
    {
        if (levelCounterText != null)
            levelCounterText.text = "Level: " + levelCount;
    }
}
