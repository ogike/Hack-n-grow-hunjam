using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public delegate void OnEnableChange(bool newValue);

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum GodModeType
    {
        Normal, GodMode, DemigodMode
    }

    public GodModeType CurrentGodModeType { get; private set; }
    
    public bool EnemyAttackEnabled { get; private set; }
    public bool EnemyMovementEnabled { get; private set; }
    
    public bool ShowAttackTriggers { get; private set; }
    
    public bool Alive { get; private set; }

    public bool showAttackTriggerDefault;
    public GodModeType godModeTypeDefault;

    public GameObject enemyToDebugSpawn;
    [Tooltip("From what distance from the player should the enemy be spawned?")]
    public float enemyToDebugSpawnDistance;
    private Transform _playerTransform;

    public OnEnableChange OnShowAttackTriggerChange;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one game manager!");
            return;
        }
        Instance = this;

        CurrentGodModeType = godModeTypeDefault;
        ShowAttackTriggers = showAttackTriggerDefault;
        EnemyAttackEnabled = true;
        EnemyMovementEnabled = true;
        Alive = true;
    }

    private void Start()
    {
        _playerTransform = PlayerController.Instance.transform;
    }

    public void Update()
    {
        if (UserInput.instance.DebugEnemySpawnPressedThisFrame)
        {
            SpawnEnemy();
        }
    }

    public void ToggleNormalMode() { CurrentGodModeType = GodModeType.Normal; }
    public void ToggleGodMode() { CurrentGodModeType = GodModeType.GodMode; }
    public void ToggleDemiGodMode() { CurrentGodModeType = GodModeType.DemigodMode; }
    
    public void ToggleEnemyAttackEnabled() { EnemyAttackEnabled = !EnemyAttackEnabled; }
    
    public void ToggleEnemyMovementEnabled() { EnemyMovementEnabled = !EnemyMovementEnabled; }

    public void ToggleShowAttackTriggers()
    {
        ShowAttackTriggers = !ShowAttackTriggers;
        
        if(OnShowAttackTriggerChange != null)
            OnShowAttackTriggerChange(ShowAttackTriggers);
    }

    public bool ChangeTime(float newValue)
    {
        if (!Alive) return false;
        
        Time.timeScale = newValue;
        return true;
    }
    
    public void SpawnEnemy()
    {
        Vector3 spawnPos = _playerTransform.position;
        spawnPos += _playerTransform.up * enemyToDebugSpawnDistance;
        
        EnemySpawner.SpawnSingleDefaultEnemy(spawnPos, enemyToDebugSpawn);
    }

    public void PlayerDeath()
    {
        Alive = false;
    }
    
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        //TODO: should not be hardcoded and rely on being first scene
        SceneManager.LoadScene(0);
    }
}
