using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHp;
    private int curHp;

    public GameObject gameOverScreen;

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
