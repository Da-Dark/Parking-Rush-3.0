using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Menu : MonoBehaviour
{
    public void OnPlayButton()
    {
        SceneManager.LoadScene(1); //level 1
    }

    public void OnQuitButton()
    {
       Application.Quit()  ; //Quit Game
    }
}
