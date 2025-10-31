using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneProgressManager : MonoBehaviour
{
    [Tooltip("Name or build index of the next map scene.")]
    public string nextSceneName = "SecondMap";  // Replace with your actual second scene name

    [Tooltip("Level number to reach before switching scenes.")]
    public int levelThreshold = 5;

    private void Update()
    {
        // Check if LevelCounterManager exists and player reached threshold
        if (LevelCounterManager.Instance != null &&
            LevelCounterManager.Instance.GetCurrentLevel() >= levelThreshold)
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        Debug.Log($"SceneProgressManager: Level {levelThreshold} reached, loading {nextSceneName}...");
        SceneManager.LoadScene(nextSceneName);
    }
}
