using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void OnPlayButton()
    {
        SceneManager.LoadScene(1); // Load level 1 or whichever scene you want
    }

    public void OnQuitButton()
    {
        Debug.Log("Quit button pressed");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // For testing inside the editor
#endif
    }
}
