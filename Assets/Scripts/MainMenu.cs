using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject creditsPanel;
    public GameObject controlsPanel;

    public void Start()
    {
        creditsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    public void ToggleCredits()
    {
        bool newValue = !creditsPanel.activeInHierarchy;
        creditsPanel.SetActive(newValue);

        if (newValue == true)
            controlsPanel.SetActive(false);
    }
    
    public void ToggleControls()
    {
        bool newValue = !controlsPanel.activeInHierarchy;
        controlsPanel.SetActive(newValue);
        
        if (newValue == true)
            creditsPanel.SetActive(false);
    }
    
    public void StartGame()
    {
        //1 has to be the main gameplay loop
        SceneManager.LoadScene(1);
    }
    
    
}
