using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHp;
    private int curHp;

    public GameObject gameOverScreen;

    [Header("UI")] 
    public Text healthText;
    public Image healthMeter;

    public void Awake()
    {
        Time.timeScale = 1; //why is the playerhealth doing this lmao
        gameOverScreen.SetActive(false);
    }

    void Start()
    {
        curHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        curHp -= damage;

        healthText.text = curHp + "/" + maxHp;
        healthMeter.fillAmount = (curHp * 1.0f) / (maxHp * 1.0f);
        
        if (curHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
