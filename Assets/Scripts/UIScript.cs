using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public static UIScript Instance { get; private set; }
    
    public Text timerText;
    public Text timescaleValueText;

    public GameObject debugPanel;
    public GameObject pausePanel;
    public GameObject gameOverScreen;
    
    public bool Paused { get; private set; }

    private float _timeScale;
    private bool _debugPanelActive;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogAssertion("More than one UIScript in scene!");
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _timeScale = 1;
        ChangeTimeSlider(1);
        HidePausePanel();
        Paused = false;
        
        gameOverScreen.SetActive(false);
    }

    private void Update()
    {
        if (UserInput.instance.DebugMenuButtonPressedThisFrame && !Paused)
        {
            ToggleDebugPanel();   
        }

        if (UserInput.instance.MenuButtonPressedThisFrame)
        {
            if (pausePanel.activeInHierarchy) HidePausePanel();
            else                              ShowPausePanel();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int allSeconds = Mathf.FloorToInt(Time.timeSinceLevelLoad);

        int minutes = allSeconds / 60;
        int secondsToDisplay = allSeconds % 60;

        timerText.text = minutes.ToString("D2") + ":" + secondsToDisplay.ToString("D2");
    }

    void ToggleDebugPanel()
    {
        debugPanel.SetActive(!debugPanel.activeInHierarchy);
    }

    public void ShowPausePanel()
    {
        if(!GameManager.Instance.Alive) return;
        
        pausePanel.SetActive(true);
        GameManager.Instance.ChangeTime(0);
        Paused = true;
        
        _debugPanelActive = pausePanel.activeInHierarchy;
        if(!_debugPanelActive)
            debugPanel.SetActive(true);
    }

    public void HidePausePanel()
    {
        if(!GameManager.Instance.Alive) return;

        pausePanel.SetActive(false);
        GameManager.Instance.ChangeTime(_timeScale);
        Paused = false;
        
        if(!debugPanel)
            debugPanel.SetActive(false);
    }

    public void ShowGameOverPanel()
    {
        gameOverScreen.SetActive(true);
    }

    public void ChangeTimeSlider(float value)
    {

        if (value < 0.05f)
        {
            Debug.LogWarning("Tried to set the Timescale lower than 0.05, invalid number!");
            return;
        }

        _timeScale = value; //this will be set in GameManager in HidePausePanel()
        timescaleValueText.text = value.ToString();

        if (!Paused) GameManager.Instance.ChangeTime(value);
    }
}
