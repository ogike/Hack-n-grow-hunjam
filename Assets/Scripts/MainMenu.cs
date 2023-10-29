using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        //1 has to be the main gameplay loop
        SceneManager.LoadScene(1);
    }
}
