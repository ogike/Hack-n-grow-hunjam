using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public List<int> maxHps;
    private int curMaxHp;
    private int curHp;

    public TimeValue invisibilityFrameTime;
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

    public void TakeDamage(int damage, Vector2 knockBackVector)
    {
        if (IsVulnerable() == false) return;

        if (_playerController.Defending)
        {
            damage = Mathf.FloorToInt((damage * 1.0f) * _playerController.defenseDamageReduction);
        }

        curHp -= damage;
        UpdateHealthUI();
        
        AudioManager.Instance.PlayAudio(takeDamageAudio);
        
        //this will derail mecanim if we are in a substate - make sure the logic side steps out too
        _playerAnimator.SetTrigger("Damaged");
        
        _playerController.PlayerGetDamage(knockBackVector);
        
        
        curIFrameTime = invisibilityFrameTime.Seconds;
        
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
        UIScript.Instance.ShowGameOverPanel();
        GameManager.Instance.PlayerDeath();
    }

    //return false if dashing or recently damaged
    public bool IsVulnerable()
    {
        if (_playerController.IsDashing()) return false;
        
        if (_gameManager.CurrentGodModeType == GameManager.GodModeType.GodMode) return false;

        if (curIFrameTime > 0) return false;

        return true;
    }

    /// <summary>
    /// Resets stats according to CurLevel
    /// Refills to Max HP
    /// </summary>
    public void Grow()
    {
        curMaxHp = maxHps[_playerController.CurLevel];
        curHp = curMaxHp;
        
        UpdateHealthUI();
    }
}
