using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public Text timerText;
    public Text timescaleValueText;

    public GameObject debugPanel;

    private void Start()
    {
        ChangeTimeSlider(1);
    }

    private void Update()
    {
        if (UserInput.instance.DebugMenuButtonPressedThisFrame)
        {
            ToggleDebugPanel();   
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

    public void ChangeTimeSlider(float value)
    {
        bool success = GameManager.Instance.ChangeTime(value);
        if(!success) return;

        timescaleValueText.text = value.ToString();
    }
}
