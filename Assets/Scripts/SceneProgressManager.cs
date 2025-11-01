using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneProgressManager : MonoBehaviour
{
    [Tooltip("Build index of the next scene in Build Settings.")]
    public int nextSceneBuildIndex = 2;

    [Tooltip("Level number required before loading the next scene.")]
    public int levelThreshold = 3;

    private bool hasLoadedNextScene = false;

    private void Update()
    {
        if (hasLoadedNextScene)
            return;

        if (LevelCounterManager.Instance == null)
            return;

        int currentLevel = LevelCounterManager.Instance.GetCurrentLevel();

        if (currentLevel >= levelThreshold)
        {
            hasLoadedNextScene = true;
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        Debug.Log($"SceneProgressManager: Level threshold reached, loading scene with index {nextSceneBuildIndex}...");
        SceneManager.LoadScene(nextSceneBuildIndex);
    }
}
