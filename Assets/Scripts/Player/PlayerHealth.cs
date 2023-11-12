using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public List<int> maxHps;
    private int curMaxHp;
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
    private Animator _playerAnimator;
    private GameManager _gameManager;
    
    public void Awake()
    {
        Time.timeScale = 1; //why is the playerhealth doing this lmao
        gameOverScreen.SetActive(false);
        
        _playerController = GetComponent<PlayerController>();
        if (_playerController == null)
        {
            Debug.LogError("No PlayerController attached");
        }

        _playerAnimator = _playerController.animator;

        curIFrameTime = 0;
    }

    void Start()
    {
        if (maxHps.Count != _playerController.maxLevel + 1) Debug.LogError("maxHps array length not matching!");
        
        _gameManager = GameManager.Instance;
        
        curMaxHp = maxHps[0];
        curHp = curMaxHp;

        UpdateHealthUI();
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

        UpdateHealthUI();
        _playerAnimator.SetTrigger("Damaged");

        curIFrameTime = invisibilityFrameTime;
        
        if (curHp <= 0)
        {
            if (_gameManager.CurrentGodModeType == GameManager.GodModeType.Normal)
            {
                Die();
            }
            else
            {
                //TODO: respawn sound?
                AudioManager.Instance.PlayAudio(playerDieAudio);
                
                curHp = curMaxHp;
                UpdateHealthUI();
            }
        }
    }

    private void UpdateHealthUI()
    {
        healthText.text = curHp + "/" + curMaxHp;
        healthMeter.fillAmount = (curHp * 1.0f) / (curMaxHp * 1.0f);
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
        
        if (_gameManager.CurrentGodModeType == GameManager.GodModeType.GodMode) return false;

        if (curIFrameTime > 0) return false;

        return true;
    }

    public void Grow()
    {
        curMaxHp = maxHps[_playerController.CurLevel];
        curHp = curMaxHp;
        
        UpdateHealthUI();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
