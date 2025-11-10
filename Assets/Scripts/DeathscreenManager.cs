using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeathscreenManager : MonoBehaviour
{
    public static DeathscreenManager Instance;

    [Header("UI References (auto-detected)")]
    public GameObject deathscreenUI;
    public Button restartButton;
    public Button quitButton;
    public TextMeshProUGUI levelReachedText;

    private bool hasSceneInitialized = false;

    private void Awake()
    {
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(InitializeSceneDependencies());
    }

    private IEnumerator InitializeSceneDependencies()
    {
        // Wait one frame to make sure all GameObjects exist
        yield return null;

        // Always refresh references on new scene load
        FindUIReferences();

        // Ensure the deathscreen is hidden on load
        if (deathscreenUI != null)
            deathscreenUI.SetActive(false);

        Time.timeScale = 1f;

        // Reactivate player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.SetActive(true);

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.enabled = true;
        }

        // Optional: ensure LevelCounterManager exists
        if (LevelCounterManager.Instance == null)
        {
            Debug.LogWarning("⚠️ LevelCounterManager not found in scene!");
        }

        hasSceneInitialized = true;
        Debug.Log($"✅ Scene initialized: {SceneManager.GetActiveScene().name}");
    }

    private void FindUIReferences()
    {
        deathscreenUI = GameObject.FindWithTag("DeathscreenUI");

        restartButton = GameObject.Find("RestartButton")?.GetComponent<Button>();
        quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();
        levelReachedText = GameObject.Find("LevelReachedText")?.GetComponent<TextMeshProUGUI>();

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitToMenu);
        }

        Debug.Log("🔄 DeathscreenManager UI references refreshed.");
    }

    public void ShowDeathscreen()
    {
        if (deathscreenUI == null)
            FindUIReferences();

        if (deathscreenUI == null)
        {
            Debug.LogError("❌ DeathscreenManager: Cannot show Deathscreen — UI not found!");
            return;
        }

        if (LevelCounterManager.Instance != null && levelReachedText != null)
        {
            int level = LevelCounterManager.Instance.GetCurrentLevel();
            levelReachedText.text = $"You reached Level {level}!";
        }

        deathscreenUI.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("💀 Deathscreen displayed.");
    }

    private void RestartGame()
    {
        Debug.Log("🔁 Restart pressed.");
        Time.timeScale = 1f;

        if (LevelCounterManager.Instance != null)
            LevelCounterManager.Instance.ResetCounter();

        // Make sure deathscreen hides before reloading
        if (deathscreenUI != null)
            deathscreenUI.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitToMenu()
    {
        Debug.Log("🚪 Quit pressed.");
        Time.timeScale = 1f;

        if (deathscreenUI != null)
            deathscreenUI.SetActive(false);

        SceneManager.LoadScene(0);
    }
}
