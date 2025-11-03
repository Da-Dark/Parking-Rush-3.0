using UnityEngine;

public class PauseTrigger : MonoBehaviour
{
    public GameObject pauseMenu; // Assign your PauseMenu GameObject here

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu != null)
            {
                // Toggle menu active state
                pauseMenu.SetActive(!pauseMenu.activeSelf);
            }
        }
    }
}
