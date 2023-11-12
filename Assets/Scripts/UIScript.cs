using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public Text timerText;

    public GameObject debugPanel;

    private void Update()
    {
        if (Input.GetButtonDown("Enable Debug"))
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
}
