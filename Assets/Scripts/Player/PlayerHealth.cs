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

    public float invisibilityFrameTime;
    private float curIFrameTime;

    [Header("UI")] 
    public Text healthText;
    public Image healthMeter;
    public AudioClip takeDamageAudio;
    public AudioClip playerDieAudio;

    private PlayerController _playerController;

    public void Awake()
    {
        Time.timeScale = 1; //why is the playerhealth doing this lmao
        gameOverScreen.SetActive(false);
        
        _playerController = GetComponent<PlayerController>();
        if (_playerController == null)
        {
            Debug.LogError("No PlayerController attached");
        }

        curIFrameTime = 0;
    }

    void Start()
    {
        curHp = maxHp;
    }

    public void Update()
    {
        if (curIFrameTime > 0)
        {
            curIFrameTime -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsVulnerable() == false) return;

        AudioManager.Instance.PlayAudio(takeDamageAudio);

        curHp -= damage;

        healthText.text = curHp + "/" + maxHp;
        healthMeter.fillAmount = (curHp * 1.0f) / (maxHp * 1.0f);

        curIFrameTime = invisibilityFrameTime;
        
        if (curHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        AudioManager.Instance.PlayAudio(playerDieAudio);
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
    }

    //return false if dashing or recently damaged
    public bool IsVulnerable()
    {
        if (_playerController.IsDashing()) return false;

        if (curIFrameTime > 0) return false;

        return true;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
