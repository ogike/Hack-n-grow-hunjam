using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public GameObject enemyToDebugSpawn;
    [Tooltip("From what distance from the player should the enemy be spawned?")]
    public float enemyToDebugSpawnDistance;
    private Transform _playerTransform;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one game manager!");
            return;
        }
        Instance = this;

        CurrentGodModeType = GodModeType.Normal;
        EnemyAttackEnabled = true;
        EnemyMovementEnabled = true;
    }

    private void Start()
    {
        _playerTransform = PlayerController.Instance.transform;
    }

    public void ToggleNormalMode() { CurrentGodModeType = GodModeType.Normal; }
    public void ToggleGodMode() { CurrentGodModeType = GodModeType.GodMode; }
    public void ToggleDemiGodMode() { CurrentGodModeType = GodModeType.DemigodMode; }
    
    public void ToggleEnemyAttackEnabled() { EnemyAttackEnabled = !EnemyAttackEnabled; }
    
    public void ToggleEnemyMovementEnabled() { EnemyMovementEnabled = !EnemyMovementEnabled; }

    public bool ChangeTime(float newValue)
    {
        if (newValue < 0.05f)
        {
            Debug.LogWarning("Tried to set the Timescale lower than 0.05, invalid number!");
            return false;
        }

        Time.timeScale = newValue;
        return true;
    }
    
    public void SpawnEnemy()
    {
        Vector3 spawnPos = _playerTransform.position;
        spawnPos += _playerTransform.up * enemyToDebugSpawnDistance;
        
        EnemySpawner.SpawnSingleDefaultEnemy(spawnPos, enemyToDebugSpawn);
    }
}
